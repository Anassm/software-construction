import pytest
import requests
from datetime import datetime, timedelta


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
                "role": "user",
                "token": "userToken123" # Toegevoegd voor de test, simuleert de token na login
            },
            "user_b": {
                "email": "user2@example.com",
                "password": "User2Pass123!",
                "username": "user.two",
                "name": "Second User",
                "role": "user",
                "token": "otherUserToken" # Token voor de 'andere' gebruiker
            },
            "admin": {
                "email": "admin@example.com",
                "password": "AdminPass123!",
                "username": "admin.user",
                "name": "Admin User",
                "role": "admin",
                "token": "adminToken123"
            }
        }
    }

BASE_URL = "http://localhost:8000/api/reservations"

@pytest.fixture
def _data():
    return {
        "user_token": "userToken123",
        "other_token": "otherUserToken",
        "admin_token": "adminToken123"
    }

@pytest.fixture
def auth_headers(_data):
    return {"Authorization": _data["user_token"]}

@pytest.fixture
def other_headers(_data):
    return {"Authorization": _data["other_token"]}

def create_dummy_reservation(headers):
    start = (datetime.now(timezone.utc) + timedelta(hours=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(hours=3)).isoformat()
    
    payload = {
        "licensePlate": "TEST-123", 
        "parkingLotId": "11111111-1111-1111-1111-111111111111", 
        "startDate": start,
        "endDate": end
    }
    r = requests.post(BASE_URL, json=payload, headers=headers)
    if r.status_code == 201:
        return r.json()
    return None


def test_create_reservation_with_license_plate(auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    payload = {
        "licensePlate": "TEST-123",
        "parkingLotId": "11111111-1111-1111-1111-111111111111",
        "startDate": start,
        "endDate": end
    }
    r = requests.post(BASE_URL, json=payload, headers=auth_headers)
    assert r.status_code == 201
    data = r.json()
    assert data["licensePlate"] == "TEST123"
    assert "id" in data

def test_create_reservation_vehicle_not_found(auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    payload = {
        "licensePlate": "NON-EXISTENT",
        "parkingLotId": "11111111-1111-1111-1111-111111111111",
        "startDate": start,
        "endDate": end
    }
    r = requests.post(BASE_URL, json=payload, headers=auth_headers)
    assert r.status_code == 400
    assert "Vehicle" in r.json().get("error", "")


def test_get_my_reservations(auth_headers):
    create_dummy_reservation(auth_headers)
    
    r = requests.get(BASE_URL, headers=auth_headers)
    assert r.status_code == 200
    data = r.json()
    assert isinstance(data, list)
    if len(data) > 0:
        res = data[0]
        assert "id" in res
        assert "vehicleId" in res
        assert "parkingLotId" in res

def test_get_reservations_unauthorized():
    r = requests.get(BASE_URL)
    assert r.status_code == 401


def test_update_reservation_success(auth_headers):
    res = create_dummy_reservation(auth_headers)
    if not res:
        pytest.skip("Kon geen reservering aanmaken voor update test")
    
    res_id = res["id"]
    new_start = (datetime.now(timezone.utc) + timedelta(days=2)).isoformat()
    new_end = (datetime.now(timezone.utc) + timedelta(days=2, hours=4)).isoformat()
    
    payload = {
        "startDate": new_start,
        "endDate": new_end
    }
    
    r = requests.put(f"{BASE_URL}/{res_id}", json=payload, headers=auth_headers)
    assert r.status_code == 200
    data = r.json()
    assert data["startDate"] == new_start

def test_update_reservation_not_found(other_headers, auth_headers):
    res = create_dummy_reservation(auth_headers)
    if not res:
        pytest.skip("Kon geen reservering aanmaken")
        
    res_id = res["id"]
    
    payload = {"startDate": (datetime.now(timezone.utc) + timedelta(days=5)).isoformat()}
    r = requests.put(f"{BASE_URL}/{res_id}", json=payload, headers=other_headers)
    
    assert r.status_code == 404

def test_update_reservation_invalid_dates(auth_headers):
    res = create_dummy_reservation(auth_headers)
    if not res:
        pytest.skip("Setup failed")
        
    res_id = res["id"]
    payload = {
        "startDate": (datetime.now(timezone.utc) + timedelta(hours=5)).isoformat(),
        "endDate": (datetime.now(timezone.utc) + timedelta(hours=4)).isoformat()
    }
    
    r = requests.put(f"{BASE_URL}/{res_id}", json=payload, headers=auth_headers)
    assert r.status_code == 400


def test_delete_reservation_success(auth_headers):
    res = create_dummy_reservation(auth_headers)
    if not res:
        pytest.skip("Setup failed")
        
    res_id = res["id"]
    
    r = requests.delete(f"{BASE_URL}/{res_id}", headers=auth_headers)
    assert r.status_code == 204
    
    r_check = requests.get(BASE_URL, headers=auth_headers)
    ids = [item["id"] for item in r_check.json()]
    assert res_id not in ids

def test_delete_reservation_not_found(other_headers, auth_headers):
    res = create_dummy_reservation(auth_headers)
    if not res:
        pytest.skip("Setup failed")
        
    res_id = res["id"]
    
    r = requests.delete(f"{BASE_URL}/{res_id}", headers=other_headers)
    assert r.status_code == 404