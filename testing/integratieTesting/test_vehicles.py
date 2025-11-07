import pytest
import requests
import uuid

@pytest.fixture
def _data():
    return {
        "base": "http://localhost:8000",
        "users": {
            "user_a": {
                "email": "user@example.com",
                "password": "UserPass123!",
                "username": "regular.user",
                "name": "Regular User",
                "role": "user"
            },
            "user_b": {
                "email": "user2@example.com",
                "password": "User2Pass123!",
                "username": "user.two",
                "name": "Second User",
                "role": "user"
            },
            "admin": {
                "email": "admin@example.com",
                "password": "AdminPass123!",
                "username": "admin.user",
                "name": "Admin User",
                "role": "admin"
            }
        }
    }

def register_and_login(base_url, user):
    requests.post(f"{base_url}/register", json=user)
    r = requests.post(f"{base_url}/login", json={"username": user["username"], "password": user["password"]})
    body = r.json()
    return {"Authorization": f"{body["tokentype"]} {body["accesstoken"]}"}

@pytest.fixture
def user_token(_data):
    return register_and_login(_data["base"], _data["users"]["user_a"])

@pytest.fixture
def user_token_b(_data):
    return register_and_login(_data["base"], _data["users"]["user_b"])

@pytest.fixture
def admin_token(_data):
    return register_and_login(_data["base"], _data["users"]["admin"])

LICENSE_PLATES = [
    "AB-123",
    "AB123",
    "XY-999",
    "XY999",
    "ZZ-999",
    "ZZ999",
    "nonexistent_id"
]

@pytest.fixture(autouse=True)
def clean_vehicles(_data, admin_token):
    for lp in LICENSE_PLATES:
        requests.delete(f"{_data['base']}/vehicles/{lp}", headers=admin_token)
    
    yield

    for lp in LICENSE_PLATES:
        requests.delete(f"{_data['base']}/vehicles/{lp}", headers=admin_token)

def vehicle_url(_data, lid=None, suffix=None):
    url = f"{_data['base']}/vehicles"
    if lid:
        url += f"/{lid}"
    if suffix:
        url += f"/{suffix}"
    return url

# POST / vehicles
def test_post_vehicle_no_auth(_data):
    response = requests.post(
        vehicle_url(_data),
        json={"name": "Toyota", "LicensePlate": "AB-123"}
    )
    assert response.status_code == 401
    assert "Unauthorized" in response.text

def test_post_vehicle_invalid_token(_data):
    response = requests.post(
        vehicle_url(_data),
        headers={"Authorization": "invalid-token"},
        json={"name": "Toyota", "LicensePlate": "AB-123"}
    )
    assert response.status_code == 401
    assert "Unauthorized" in response.text or "Invalid" in response.text

def test_post_missing_field_license_plate(_data, user_token):
    payload = {"name": "Toyota"} 
    response = requests.post(vehicle_url(_data), headers=user_token, json=payload)
    assert response.status_code == 400
    body = response.json()
    assert "errors" in body
    error_string = body["errors"]["$"][0]
    assert "licensePlate" in error_string

def test_post_invalid_json(_data, user_token):
    response = requests.post(
        vehicle_url(_data),
        headers={**user_token, "Content-Type": "application/json"},
        data="{ invalid json"
    )
    assert response.status_code == 400

def test_post_create_vehicle_success(_data, user_token):
    lp = f"AB-{str(uuid.uuid4())[:6]}"
    payload = {"name": "Toyota", "LicensePlate": lp}
    response = requests.post(vehicle_url(_data), headers=user_token, json=payload)
    assert response.status_code == 201
    body = response.json()
    assert body is not None

def test_post_duplicate_vehicle(_data, user_token):
    payload = {"name": "Toyota", "LicensePlate": "AB-123"}
    requests.post(vehicle_url(_data), headers=user_token, json=payload)
    response = requests.post(vehicle_url(_data), headers=user_token, json=payload)
    assert response.status_code == 409
    body = response.json()
    assert body is not None

#put /Vehicles
def test_put_vehicle_no_auth(_data):
    url = vehicle_url(_data, "AB-123")
    response = requests.put(url, json={"name": "Toyota", "LicensePlate": "AB-123"})
    assert response.status_code == 401
    assert "Unauthorized" in response.text

def test_put_vehicle_invalid_token(_data):
    url = vehicle_url(_data, "AB-123")
    response = requests.put(url, headers={"Authorization": "invalid-token"}, json={"name": "Toyota", "LicensePlate": "AB-123"})
    assert response.status_code == 401

def test_put_vehicle_missing_field(_data, user_token):
    url = vehicle_url(_data, "AB-123")
    response = requests.put(url, headers=user_token, json={}) 
    assert response.status_code == 400
    body = response.json()
    assert "Request must contain a body" in body["error"] or "LicensePlate" in body["error"]

def test_put_vehicle_invalid_json(_data, user_token):
    url = vehicle_url(_data, "AB-123")
    headers = {**user_token, "Content-Type": "application/json"}
    response = requests.put(url, headers=headers, data="{ invalid json")
    assert response.status_code == 400

def test_put_vehicle_create_new(_data, user_token):
    url = vehicle_url(_data, "XY-999")
    payload = {"name": "Tesla Model 3", "LicensePlate": "XY-999"}
    response = requests.put(url, headers=user_token, json=payload)
    assert response.status_code == 404
    body = response.json()
    assert body is not None
    if "vehicle" in body:
        assert body["vehicle"]["name"] == "Tesla Model 3"

def test_put_vehicle_update_existing_success(_data, user_token):
    create_payload = {"name": "Toyota", "LicensePlate": "AB-123"}
    requests.post(vehicle_url(_data), headers=user_token, json=create_payload)
    update_payload = {"name": "Toyota Supra", "LicensePlate": "AB-123"}
    url = vehicle_url(_data, "AB-123")
    response = requests.put(url, headers=user_token, json=update_payload)
    assert response.status_code == 200

#delete /vehicles
def test_delete_vehicle_no_auth(_data):
    url = vehicle_url(_data, "AB-123")
    response = requests.delete(url)
    assert response.status_code == 401
    assert "Unauthorized" in response.text

def test_delete_vehicle_invalid_token(_data):
    url = vehicle_url(_data, "AB-123")
    response = requests.delete(url, headers={"Authorization": "invalid-token"})
    assert response.status_code == 401

def test_delete_vehicle_not_found(_data, user_token):
    url = vehicle_url(_data, "ZZ-999")
    response = requests.delete(url, headers=user_token)
    assert response.status_code == 404
    body = response.json()
    assert body is not None

def test_delete_vehicle_success(_data, user_token):
    payload = {"name": "Toyota", "LicensePlate": "AB-123"}
    requests.post(vehicle_url(_data), headers=user_token, json=payload)
    url = vehicle_url(_data, "AB-123")
    response = requests.delete(url, headers=user_token)
    assert response.status_code == 200
    body = response.json()
    assert body is not None

def test_delete_vehicle_not_found(_data, user_token):
    payload = {"name": "Toyota", "LicensePlate": "AB-123"}
    requests.post(vehicle_url(_data), headers=user_token, json=payload)
    url = vehicle_url(_data, "AB-123")
    requests.delete(url, headers=user_token)
    response = requests.delete(url, headers=user_token)
    assert response.status_code == 404
    body = response.json()
    assert body is not None

def test_delete_vehicle_different_user_cannot_delete(_data, user_token, user_token_b):
    payload = {"name": "Toyota", "LicensePlate": "AB-123"}
    requests.post(vehicle_url(_data), headers=user_token, json=payload)
    url = vehicle_url(_data, "AB-123")
    response = requests.delete(url, headers=user_token_b)
    assert response.status_code == 404
    body = response.json()
    assert body is not None

#Get /vehicles
def test_get_vehicles_no_auth(_data):
    response = requests.get(vehicle_url(_data))
    assert response.status_code == 401
    assert "Unauthorized" in response.text

def test_get_vehicles_invalid_token(_data):
    response = requests.get(vehicle_url(_data), headers={"Authorization": "invalid-token"})
    assert response.status_code == 401

def test_get_vehicles_success(_data, user_token):
    response = requests.get(vehicle_url(_data), headers=user_token)
    assert response.status_code == 200
    data = response.json()
    assert isinstance(data, list) or isinstance(data, dict)

def test_get_vehicle_reservations_not_found(_data, user_token):
    url = vehicle_url(_data, "nonexistent_id", "reservations")
    response = requests.get(url, headers=user_token)
    assert response.status_code == 404

def test_get_vehicle_reservations_success(_data, user_token):
    payload = {"name": "Toyota", "LicensePlate": f"AB-{str(uuid.uuid4())[:4]}"}
    rqResponse = requests.post(vehicle_url(_data), headers=user_token, json=payload)
    body = rqResponse.json()
    vid = body["vehicle"]["id"]
    url = vehicle_url(_data, vid, "reservations")
    response = requests.get(url, headers=user_token)
    assert response.status_code == 200
    body = response.json()
    assert isinstance(body, list)

def test_get_vehicle_history_not_found(_data, user_token):
    url = vehicle_url(_data, "nonexistent_id", "history")
    response = requests.get(url, headers=user_token)
    assert response.status_code == 404

def test_get_vehicle_history_success(_data, user_token):
    payload = {"name": "Toyota", "LicensePlate": f"AB-{str(uuid.uuid4())[:4]}"}
    rqResponse = requests.post(vehicle_url(_data), headers=user_token, json=payload)
    body = rqResponse.json()
    vid = body["vehicle"]["id"]
    url = vehicle_url(_data, vid, "history")
    response = requests.get(url, headers=user_token)
    assert response.status_code == 200
    body = response.json()
    assert isinstance(body, list)