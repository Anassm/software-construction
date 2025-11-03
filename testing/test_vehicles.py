# import pytest
# import requests
# import json
# import sys
# import os
# sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))

# from session_manager import add_session, remove_session



# @pytest.fixture
# def _data():
#     return {'url': 'http://localhost:8000/vehicles'}

# @pytest.fixture
# def valid_token_a():
#     token = "valid-session-token-userA"
#     add_session(token, {"username": "userA"})
#     yield token
#     remove_session(token)

# @pytest.fixture
# def valid_token_b():
#     token = "valid-session-token-userB"
#     add_session(token, {"username": "userB"})
#     yield token
#     remove_session(token)

# @pytest.fixture
# def headers_user_a(valid_token_a):
#     return {"Authorization": valid_token_a, "Content-Type": "application/json"}

# @pytest.fixture
# def headers_user_b(valid_token_b):
#     return {"Authorization": valid_token_b, "Content-Type": "application/json"}


# # POST / vehicles
# def test_post_vehicle_no_auth(_data):
#     response = requests.post(_data['url'], json={"name": "Toyota", "license_plate": "AB-123"})
#     assert response.status_code == 401
#     assert "Unauthorized" in response.text

# def test_post_vehicle_invalid_token(_data):
#     response = requests.post(
#         _data['url'],
#         headers={"Authorization": "invalid-token"},
#         json={"name": "Toyota", "license_plate": "AB-123"}
#     )
#     assert response.status_code == 401
#     assert "Invalid" in response.text or "Unauthorized" in response.text


# def test_post_missing_field_license_plate(_data, headers_user_a):
#     payload = {"name": "Toyota"}
#     missing_field = "license_plate"
#     response = requests.post(_data['url'], headers=headers_user_a, json=payload)
#     assert response.status_code == 400
#     body = response.json()
#     assert body["error"] == "Required field missing"
#     assert body["field"] == missing_field

# def test_post_missing_field_name(_data, headers_user_a):
#     payload = {"license_plate": "AB-123"}
#     missing_field = "name"
#     response = requests.post(_data['url'], headers=headers_user_a, json=payload)
#     assert response.status_code == 400
#     body = response.json()
#     assert body["error"] == "Required field missing"
#     assert body["field"] == missing_field

# def test_post_invalid_json(_data, headers_user_a):
#     response = requests.post(
#         _data['url'],
#         headers=headers_user_a,
#         data="{ invalid json"
#     )
#     assert response.status_code == 400

# def test_post_create_vehicle_success(_data, headers_user_a):
#     payload = {"name": "Toyota", "license_plate": "AB-123"}
#     response = requests.post(_data['url'], headers=headers_user_a, json=payload)
#     assert response.status_code == 201
#     body = response.json()
#     assert body["status"] == "Success"
#     assert body["vehicle"]["name"] == payload["name"]
#     assert body["vehicle"]["license_plate"] == payload["license_plate"]

# def test_post_duplicate_vehicle(_data, headers_user_a):
#     payload = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=payload)
#     response = requests.post(_data['url'], headers=headers_user_a, json=payload)
#     assert response.status_code == 409
#     body = response.json()
#     assert "Vehicle already exists" in body["error"]


# #POST /vehicles/
# def test_post2_vehicle_no_auth(_data):
#     url = _data['url'] + "/AB-123/entry"
#     response = requests.post(url, json={"parkinglot": "1"})
#     assert response.status_code == 401
#     assert "Unauthorized" in response.text

# def test_post2_vehicle_invalid_token(_data):
#     url = _data['url'] + "/AB-123/entry"
#     response = requests.post(url, headers={"Authorization": "invalid"}, json={"parkinglot": "1"})
#     assert response.status_code == 401

# def test_post2_vehicle_missing_field(_data, headers_user_a):
#     url = _data['url'] + "/AB-123/entry"
#     response = requests.post(url, headers=headers_user_a, json={})
#     assert response.status_code == 400
#     body = response.json()
#     assert body["error"] == "Required field missing"
#     assert body["field"] == "parkinglot"

# def test_post2_vehicle_not_exists(_data, headers_user_a):
#     url = _data['url'] + "/NONEXIST/entry"
#     response = requests.post(url, headers=headers_user_a, json={"parkinglot": "1"})
#     assert response.status_code == 404
#     body = response.json()
#     assert body["error"] == "Vehicle does not exist"

# def test_post2_vehicle_success(_data, headers_user_a):
#     payload_create = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=payload_create)

#     url = _data['url'] + "/AB-123/entry"
#     response = requests.post(url, headers=headers_user_a, json={"parkinglot": "1"})
#     assert response.status_code == 200
#     body = response.json()
#     assert body["status"] == "Accepted"
#     assert body["vehicle"]["licenseplate"] == "AB-123"

# def test_post2_vehicle_invalid_json(_data, headers_user_a):
#     url = _data['url'] + "/AB-123/entry"
#     response = requests.post(url, headers=headers_user_a, data="{ invalid json")
#     assert response.status_code == 400


# #PUT /vehicles/
# def test_put_vehicle_no_auth(_data):
#     url = _data['url'] + "/AB-123"
#     response = requests.put(url, json={"name": "Toyota"})
#     assert response.status_code == 401
#     assert "Unauthorized" in response.text

# def test_put_vehicle_invalid_token(_data):
#     url = _data['url'] + "/AB-123"
#     response = requests.put(url, headers={"Authorization": "invalid-token"}, json={"name": "Toyota"})
#     assert response.status_code == 401

# def test_put_vehicle_missing_field(_data, headers_user_a):
#     url = _data['url'] + "/AB-123"
#     response = requests.put(url, headers=headers_user_a, json={})
#     assert response.status_code == 400
#     body = response.json()
#     assert body["error"] == "Required field missing"
#     assert body["field"] == "name"

# def test_put_vehicle_invalid_json(_data, headers_user_a):
#     url = _data['url'] + "/AB-123"
#     response = requests.put(url, headers=headers_user_a, data="{ invalid json")
#     assert response.status_code == 400

# def test_put_vehicle_create_new(_data, headers_user_a):
#     url = _data['url'] + "/XY-999"
#     payload = {"name": "Tesla Model 3", "license_plate": "XY-999"}
#     response = requests.put(url, headers=headers_user_a, json=payload)
#     assert response.status_code == 201
#     body = response.json()
#     assert "vehicle" in body
#     assert body["vehicle"]["name"] == "Tesla Model 3"

# def test_put_vehicle_update_existing(_data, headers_user_a):
#     create_payload = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=create_payload)
#     update_payload = {"name": "Toyota Supra"}
#     url = _data['url'] + "/AB-123"
#     response = requests.put(url, headers=headers_user_a, json=update_payload)
#     assert response.status_code == 200
#     body = response.json()
#     assert body["vehicle"]["name"] == "Toyota Supra"


# #DELETE /vehicles/
# def test_delete_vehicle_no_auth(_data):
#     url = _data['url'] + "/AB-123"
#     response = requests.delete(url)
#     assert response.status_code == 401
#     assert "Unauthorized" in response.text

# def test_delete_vehicle_invalid_token(_data):
#     url = _data['url'] + "/AB-123"
#     response = requests.delete(url, headers={"Authorization": "invalid-token"})
#     assert response.status_code == 401

# def test_delete_vehicle_not_found(_data, headers_user_a):
#     url = _data['url'] + "/ZZ-999"
#     response = requests.delete(url, headers=headers_user_a)
#     assert response.status_code == 404
#     assert "Vehicle not found" in response.text

# def test_delete_vehicle_success(_data, headers_user_a):
#     """Eerst voertuig aanmaken, dan verwijderen."""
#     payload = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=payload)
#     url = _data['url'] + "/AB-123"
#     response = requests.delete(url, headers=headers_user_a)
#     assert response.status_code == 200
#     body = response.json()
#     assert body["status"] == "Deleted"

# def test_delete_vehicle_twice_returns_404(_data, headers_user_a):
#     """Tweemaal verwijderen → tweede keer moet 404 geven."""
#     payload = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=payload)
#     url = _data['url'] + "/AB-123"
#     requests.delete(url, headers=headers_user_a)
#     response = requests.delete(url, headers=headers_user_a)
#     assert response.status_code == 404

# def test_delete_vehicle_different_user_cannot_delete(_data, headers_user_a, headers_user_b):
#     """User B mag voertuig van A niet verwijderen."""
#     payload = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=payload)
#     url = _data['url'] + "/AB-123"
#     response = requests.delete(url, headers=headers_user_b)
#     assert response.status_code in [403, 404]


# #GET /vehicles
# def test_get_vehicles_no_auth(_data):
#     response = requests.get(_data['url'])
#     assert response.status_code == 401
#     assert "Unauthorized" in response.text

# def test_get_vehicles_invalid_token(_data):
#     response = requests.get(_data['url'], headers={"Authorization": "invalid-token"})
#     assert response.status_code == 401

# def test_get_vehicles_success(headers_user_a, _data):
#     response = requests.get(_data['url'], headers=headers_user_a)
#     assert response.status_code == 200
#     assert isinstance(response.json(), dict)

# def test_get_vehicle_reservations_not_found(headers_user_a, _data):
#     url = f"{_data['url']}/nonexistent_id/reservations"
#     response = requests.get(url, headers=headers_user_a)
#     assert response.status_code == 404
#     assert "Not found" in response.text

# def test_get_vehicle_reservations_success(headers_user_a, _data):
#     payload = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=payload)
#     url = f"{_data['url']}/AB-123/reservations"
#     response = requests.get(url, headers=headers_user_a)
#     assert response.status_code == 200
#     assert response.json() == []

# def test_get_vehicle_history_not_found(headers_user_a, _data):
#     url = f"{_data['url']}/nonexistent_id/history"
#     response = requests.get(url, headers=headers_user_a)
#     assert response.status_code == 404

# def test_get_vehicle_history_success(headers_user_a, _data):
#     payload = {"name": "Toyota", "license_plate": "AB-123"}
#     requests.post(_data['url'], headers=headers_user_a, json=payload)
#     url = f"{_data['url']}/AB-123/history"
#     response = requests.get(url, headers=headers_user_a)
#     assert response.status_code == 200
#     assert response.json() == []
