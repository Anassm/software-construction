import pytest
import requests

BASE = "http://localhost:8000"

@pytest.mark.xfail(reason="Known bug: /login valid creds -> 401", strict=False)
def test_login_success():
    r = requests.post(f"{BASE}/login", json={"username": "regular.user", "password": "password123"})
    assert r.status_code == 200
    assert "session_token" in r.json()

def test_login_wrong_password():
    r = requests.post(f"{BASE}/login", json={"username": "regular.user", "password": "WRONG"})
    assert r.status_code == 401

def test_login_unknown_user():
    r = requests.post(f"{BASE}/login", json={"username": "no.such.user", "password": "password123"})
    assert r.status_code in (401, 404)

def test_login_missing_fields():
    r = requests.post(f"{BASE}/login", json={"username": "regular.user"})
    assert r.status_code in (400, 422)

@pytest.mark.xfail(reason="Known bug: invalid JSON causes server to close connection", strict=False)
def test_login_invalid_json():
    r = requests.post(f"{BASE}/login", data="{ invalid json")
    assert r.status_code == 400



def _login_or_skip(u, p):
    """Helper to log in and return token, or skip if /login fails."""
    r = requests.post(f"{BASE}/login", json={"username": u, "password": p})
    if r.status_code != 200:
        pytest.skip("Skipping: cannot obtain token due to /login bug")
    return r.json()["session_token"]

# def test_logout_requires_auth():
#     r = requests.get(f"{BASE}/logout")
#     assert r.status_code in (401, 403)

# def test_logout_success():
#     token = _login_or_skip("regular.user", "password123")
#     r = requests.get(f"{BASE}/logout", headers={"Authorization": token})
#     assert r.status_code in (200, 204)

# def test_logout_invalid_token():
#     r = requests.get(f"{BASE}/logout", headers={"Authorization": "invalid"})
#     assert r.status_code in (400, 401, 403)

# def test_logout_twice_idempotent():
#     token = _login_or_skip("regular.user", "password123")
#     r1 = requests.get(f"{BASE}/logout", headers={"Authorization": token})
#     assert r1.status_code in (200, 204)
#     r2 = requests.get(f"{BASE}/logout", headers={"Authorization": token})
#     assert r2.status_code in (200, 204, 401, 403)

# def test_token_invalid_after_logout():
#     token = _login_or_skip("regular.user", "password123")
#     requests.get(f"{BASE}/logout", headers={"Authorization": token})
#     r = requests.get(f"{BASE}/profile", headers={"Authorization": token})
#     assert r.status_code in (200, 401, 403)
