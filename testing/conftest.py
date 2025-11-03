# conftest.py
import subprocess
import time
import pytest
import os
import signal
import requests


BASE_URL = "http://localhost:8000"

@pytest.fixture(scope="session", autouse=True)
def start_api_v2_test_env():
    api_folder = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "v2"))

    
    print("Building v2 API with TEST symbol")
    subprocess.run(
        ["dotnet", "build", "-p:DefineConstants=TEST"],
        cwd=api_folder,
        check=True
    )
    process = subprocess.Popen(["dotnet", "run", "--project", "../v2", "--urls", "http://localhost:8000", "-p:DefineConstants=TEST"])
    time.sleep(2)  # wait for API to start

    yield
    process.terminate()
    process.wait(timeout=5)

@pytest.fixture
def admin_token():
    admin = {
        "email": "admin@test.com",
        "password": "Admin123!"
    }
    
    resp = requests.post(f"{BASE_URL}/login", json=admin)
    resp.raise_for_status()  

    # Extract token from response JSON (adjust if your API returns cookies instead)
    data = resp.json()
    token = data.get("accessToken")
    if not token:
        raise Exception("Login failed: no token returned")
    
    return token

@pytest.fixture
def user_token():
    user = {
        "email": "user@test.com",
        "password": "User123!"
    }
    resp = requests.post(f"{BASE_URL}/login", json=user)
    resp.raise_for_status()

    data = resp.json()
    token = data.get("accessToken")
    if not token:
        raise Exception("Login failed: no token returned")
    return token