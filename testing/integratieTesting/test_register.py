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
        "password": "Password123.",
        "name": "New User"
    })
    assert r.status_code in [200, 201]

def test_register_missing_username(_data):
    r = requests.post(_data["url"], json={
        "password": "Password123.",
        "name": "No Username"
    })
    assert r.status_code in [400, 422]

def test_register_missing_password(_data):
    r = requests.post(_data["url"], json={"username": "np.user", "name": "No Pass"})
    assert r.status_code in [400, 422]

def test_register_missing_name(_data):
    r = requests.post(_data["url"], json={"username": "noname.user", "password": "password123"})
    assert r.status_code in [400, 422]

def test_register_invalid_json(_data):
    r = requests.post(_data["url"], data="{ invalid json")
    assert r.status_code in [400, 415]

def test_register_duplicate_username(_data):
    payload = {"username": "new.user", "password": "password123", "name": "Dup User"}
    requests.post(_data["url"], json=payload)
    r2 = requests.post(_data["url"], json=payload)
    assert r2.status_code in [409, 400, 500]

def test_register_weak_password(_data):
    r = requests.post(_data["url"], json={
        "username": "weak.user",
        "password": "123",
        "name": "Weak Password"
    })
    print(f"hellol{r.text}")
    assert r.status_code in [400, 422]

def test_register_wrong_content_type(_data):
    r = requests.post(_data["url"], data="username=test&password=1234")
    assert r.status_code in [400, 415]

def test_register_short_password(_data):
    r = requests.post(_data["url"], json={
        "username": "shortpass.user",
        "password": "1234",
        "name": "Short Pass"
    })
    print(f"hell{r.text}")
    assert r.status_code in [400, 422]
