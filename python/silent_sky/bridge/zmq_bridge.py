"""ZeroMQ bridge for Python-Unity communication"""

import json
import time
from typing import Dict, Optional, Callable
import zmq
from threading import Thread

from ..env.observatory_env import ObservatoryEnv
from ..env.state import EnvironmentState


class ZMQBridge:
    """
    ZeroMQ bridge for sending state to Unity and receiving directives
    
    Python side: PUB for state updates, REP for directives
    """
    
    def __init__(
        self,
        pub_port: int = 5555,
        rep_port: int = 5556,
        enabled: bool = True
    ):
        self.pub_port = pub_port
        self.rep_port = rep_port
        self.enabled = enabled
        
        self.context = None
        self.pub_socket = None
        self.rep_socket = None
        self.running = False
        self.thread = None
        
        # Callback for mission directives
        self.directive_callback: Optional[Callable[[Dict], None]] = None
    
    def start(self):
        """Start ZeroMQ server"""
        if not self.enabled:
            return
        
        self.context = zmq.Context()
        
        # PUB socket for state updates (Python → Unity)
        self.pub_socket = self.context.socket(zmq.PUB)
        self.pub_socket.bind(f"tcp://*:{self.pub_port}")
        time.sleep(0.1)  # Give socket time to bind
        
        # REP socket for directives (Unity → Python)
        self.rep_socket = self.context.socket(zmq.REP)
        self.rep_socket.bind(f"tcp://*:{self.rep_port}")
        
        # Start directive handler thread
        self.running = True
        self.thread = Thread(target=self._handle_directives, daemon=True)
        self.thread.start()
        
        print(f"ZeroMQ bridge started: PUB on {self.pub_port}, REP on {self.rep_port}")
    
    def stop(self):
        """Stop ZeroMQ server"""
        self.running = False
        if self.thread:
            self.thread.join(timeout=1.0)
        
        if self.pub_socket:
            self.pub_socket.close()
        if self.rep_socket:
            self.rep_socket.close()
        if self.context:
            self.context.term()
    
    def send_state(self, state: EnvironmentState, observation: Dict, info: Dict):
        """Send state update to Unity"""
        if not self.enabled or not self.pub_socket:
            return
        
        # Create state snapshot with version
        snapshot = {
            "schema_version": 1,
            "timestep": state.timestep,
            "state": {
                "sectors": [
                    {
                        "sector_id": s.sector_id,
                        "sensor_reading": float(s.sensor_reading),
                        "sensor_confidence": float(s.sensor_confidence),
                        "activity_rate": float(s.activity_rate)  # For visualization only
                    }
                    for s in state.sectors
                ],
                "events": [
                    {
                        "event_type": e.event_type,
                        "sector": e.sector,
                        "timestep": e.timestep,
                        "value": float(e.value),
                        "discovered": e.discovered
                    }
                    for e in state.events
                ],
                "discovered_events": [
                    {
                        "event_type": e.event_type,
                        "sector": e.sector,
                        "value": float(e.value)
                    }
                    for e in state.discovered_events
                ],
                "budget": float(state.budget),
                "total_earnings": float(state.total_earnings),
                "total_costs": float(state.total_costs),
                "upgrades": state.upgrades.copy(),
                "time_remaining": float(state.get_time_remaining())
            },
            "observation": {
                "sensor_readings": observation["sensor_readings"].tolist(),
                "sensor_confidence": observation["sensor_confidence"].tolist(),
                "time_remaining": observation["time_remaining"].tolist(),
                "budget_remaining": observation["budget_remaining"].tolist()
            },
            "info": info
        }
        
        try:
            message = json.dumps(snapshot)
            self.pub_socket.send_string(message)
        except Exception as e:
            print(f"Error sending state: {e}")
    
    def _handle_directives(self):
        """Handle mission directives from Unity (runs in thread)"""
        while self.running:
            try:
                # Non-blocking receive
                if self.rep_socket.poll(100, zmq.POLLIN):
                    message = self.rep_socket.recv_string()
                    directive = json.loads(message)
                    
                    # Process directive
                    if self.directive_callback:
                        self.directive_callback(directive)
                    
                    # Send acknowledgment
                    response = {"status": "ok"}
                    self.rep_socket.send_string(json.dumps(response))
            except zmq.Again:
                continue
            except Exception as e:
                print(f"Error handling directive: {e}")
                if self.rep_socket:
                    try:
                        self.rep_socket.send_string(json.dumps({"status": "error", "message": str(e)}))
                    except:
                        pass
    
    def set_directive_callback(self, callback: Callable[[Dict], None]):
        """Set callback for processing mission directives"""
        self.directive_callback = callback

