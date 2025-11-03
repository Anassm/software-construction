import pytest
import requests

@pytest.fixture
def _data():
    return {
        "url": "http://localhost:8000/register"
    }

def test_register_success(_data):
    r = requests.post(_data["url"], json={
        "username": "new.user",
        "password": "password123",
        "name": "New User"
    })
    assert r.status_code in [200, 201]

def test_register_missing_password(_data):
    r = requests.post(_data["url"], json={"username": "np.user", "name": "No Pass"})
    assert r.status_code in [400, 422]

def test_register_missing_name(_data):
    r = requests.post(_data["url"], json={"username": "noname.user", "password": "password123"})
    assert r.status_code in [400, 422]

def test_register_invalid_json(_data):
    r = requests.post(_data["url"], data="{ invalid json")
    assert r.status_code == 400

def test_register_duplicate_username(_data):
    payload = {"username": "dup.user", "password": "password123", "name": "Dup User"}
    requests.post(_data["url"], json=payload)
    r2 = requests.post(_data["url"], json=payload)
    assert r2.status_code in [409, 400, 500]
