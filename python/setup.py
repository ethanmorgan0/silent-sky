"""Setup script for Silent Sky"""

from setuptools import setup, find_packages

setup(
    name="silent-sky",
    version="0.1.0",
    description="Space observatory POMDP environment",
    packages=find_packages(),
    install_requires=[
        "gymnasium>=0.29.0",
        "numpy>=1.24.0",
        "pyyaml>=6.0",
        "pyzmq>=25.0.0"
    ],
    python_requires=">=3.8"
)

