import pytest
import requests
import uuid

BASE_URL = "http://localhost:8000"
REGISTER_URL = f"{BASE_URL}/register"
LOGIN_URL = f"{BASE_URL}/login"

@pytest.fixture(scope="session")
def create_user():
    """Registers and logs in a new user, returns username + auth headers."""
    username = f"testuser_{uuid.uuid4().hex[:6]}"
    password = "TestPass123!"

    # Register
    reg = requests.post(REGISTER_URL, json={"username": username, "password": password})
    assert reg.status_code in [200, 201], f"Register failed: {reg.text}"

    # Login
    login = requests.post(LOGIN_URL, json={"username": username, "password": password})
    assert login.status_code == 200, f"Login failed: {login.text}"

    token = login.json().get("token")
    assert token, f"No token returned in login: {login.text}"

    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    return {"username": username, "headers": headers}


# ------------------ POST /vehicles ------------------

def test_post_vehicle_success(create_user):
    payload = {"name": "Toyota", "license_plate": "AB-123"}
    r = requests.post(BASE_URL, headers=create_user["headers"], json=payload)
    assert r.status_code == 201
    data = r.json()
    assert data["vehicle"]["license_plate"] == "AB-123"

def test_post_vehicle_no_auth():
    r = requests.post(BASE_URL, json={"name": "Toyota", "license_plate": "AB-123"})
    assert r.status_code == 401

def test_post_vehicle_missing_field(create_user):
    r = requests.post(BASE_URL, headers=create_user["headers"], json={"name": "Toyota"})
    assert r.status_code == 400
    assert "LicensePlate" in r.text

def test_post_vehicle_duplicate(create_user):
    payload = {"name": "Toyota", "license_plate": "AB-123"}
    requests.post(BASE_URL, headers=create_user["headers"], json=payload)
    r = requests.post(BASE_URL, headers=create_user["headers"], json=payload)
    assert r.status_code == 409

def test_post_vehicle_invalid_json(create_user):
    r = requests.post(BASE_URL, headers=create_user["headers"], data="{ invalid json")
    assert r.status_code == 400

# ------------------ GET /vehicles ------------------

def test_get_vehicles_success(create_user):
    r = requests.get(BASE_URL, headers=create_user["headers"])
    assert r.status_code == 200
    assert isinstance(r.json(), dict) or isinstance(r.json(), list)

def test_get_vehicles_no_auth():
    r = requests.get(BASE_URL)
    assert r.status_code == 401

def test_get_vehicles_invalid_token():
    headers = {"Authorization": "Bearer invalid", "Content-Type": "application/json"}
    r = requests.get(BASE_URL, headers=headers)
    assert r.status_code == 401

def test_get_vehicles_empty(create_user):
    r = requests.get(BASE_URL, headers=create_user["headers"])
    assert r.status_code in [200, 404]  # depending on implementation

def test_get_vehicles_format(create_user):
    r = requests.get(BASE_URL, headers=create_user["headers"])
    assert r.status_code == 200
    body = r.json()
    assert isinstance(body, (list, dict))

# ------------------ PUT /vehicles/{lid} ------------------

def test_put_vehicle_update_success(create_user):
    payload = {"name": "Honda", "license_plate": "XY-111"}
    r = requests.post(BASE_URL, headers=create_user["headers"], json=payload)
    assert r.status_code == 201

    update = {"name": "Honda Civic", "license_plate": "XY-111"}
    url = f"{BASE_URL}/XY-111"
    r2 = requests.put(url, headers=create_user["headers"], json=update)
    assert r2.status_code == 200
    assert "Civic" in r2.text

def test_put_vehicle_no_auth():
    r = requests.put(f"{BASE_URL}/AB-123", json={"name": "Test"})
    assert r.status_code == 401

def test_put_vehicle_missing_field(create_user):
    r = requests.put(f"{BASE_URL}/ZZ-999", headers=create_user["headers"], json={})
    assert r.status_code == 400

def test_put_vehicle_invalid_json(create_user):
    r = requests.put(f"{BASE_URL}/ZZ-999", headers=create_user["headers"], data="{ invalid")
    assert r.status_code == 400

def test_put_vehicle_not_found(create_user):
    payload = {"name": "Ford", "license_plate": "XX-777"}
    r = requests.put(f"{BASE_URL}/XX-777", headers=create_user["headers"], json=payload)
    assert r.status_code in [201, 404]

# ------------------ DELETE /vehicles/{lid} ------------------

def test_delete_vehicle_success(create_user):
    payload = {"name": "BMW", "license_plate": "DD-555"}
    requests.post(BASE_URL, headers=create_user["headers"], json=payload)
    r = requests.delete(f"{BASE_URL}/DD-555", headers=create_user["headers"])
    assert r.status_code == 200

def test_delete_vehicle_no_auth():
    r = requests.delete(f"{BASE_URL}/DD-555")
    assert r.status_code == 401

def test_delete_vehicle_invalid_token():
    headers = {"Authorization": "Bearer invalid"}
    r = requests.delete(f"{BASE_URL}/DD-555", headers=headers)
    assert r.status_code == 401

def test_delete_vehicle_not_found(create_user):
    r = requests.delete(f"{BASE_URL}/ZZ-999", headers=create_user["headers"])
    assert r.status_code == 404

def test_delete_vehicle_twice(create_user):
    payload = {"name": "Opel", "license_plate": "OP-222"}
    requests.post(BASE_URL, headers=create_user["headers"], json=payload)
    requests.delete(f"{BASE_URL}/OP-222", headers=create_user["headers"])
    r = requests.delete(f"{BASE_URL}/OP-222", headers=create_user["headers"])
    assert r.status_code == 404
