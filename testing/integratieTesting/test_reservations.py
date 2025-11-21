import pytest
import requests
from datetime import datetime, timedelta, timezone

# --- CONFIGURATIE ---

@pytest.fixture
def _data():
    return {
        "base": "http://localhost:8000",
        "users": {
            "user_a": {
                "token": "userToken123",
                "username": "regular.user"
            },
            "user_b": {
                "token": "otherUserToken",
                "username": "other.user"
            }
        },
        "parkingLotId": "11111111-1111-1111-1111-111111111111", 
        "licensePlate": "TEST-123"
    }

@pytest.fixture
def auth_headers(_data):
    return {"Authorization": _data["users"]["user_a"]["token"]}

@pytest.fixture
def other_headers(_data):
    return {"Authorization": _data["users"]["user_b"]["token"]}

def create_dummy_reservation(headers):
    start = (datetime.now(timezone.utc) + timedelta(hours=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(hours=3)).isoformat()
    
    payload = {
        "licensePlate": _data["licensePlate"],
        "parkingLotId": pl_id,
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
    
    assert r.status_code == 201
    data = r.json()
    assert data["licensePlate"] == "TEST123"

def test_create_reservation_vehicle_not_found(auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    payload = {
        "licensePlate": "NON-EXISTENT",
        "parkingLotId": _data["parkingLotId"],
        "startDate": start,
        "endDate": end
    }
    r = requests.post(reservations_url, json=payload, headers=auth_headers)
    
    assert r.status_code == 400

def test_get_my_reservations(reservations_url, auth_headers):
    r = requests.get(reservations_url, headers=auth_headers)
    
    assert r.status_code == 200
    assert isinstance(r.json(), list)

def test_get_reservations_unauthorized(reservations_url):
    r = requests.get(reservations_url)
    assert r.status_code == 401

def test_update_reservation_success(reservations_url, auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    res_id = res["id"]
    new_start = (datetime.now(timezone.utc) + timedelta(days=2)).isoformat()
    new_end = (datetime.now(timezone.utc) + timedelta(days=2, hours=4)).isoformat()
    
    payload = {
        "startDate": new_start,
        "endDate": new_end
    }
    
    r = requests.put(f"{reservations_url}/{res_id}", json=payload, headers=auth_headers)
    
    assert r.status_code == 200
    data = r.json()
    assert data["startDate"] == new_start

def test_update_reservation_not_found_or_owned(reservations_url, other_headers, auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    payload = {"startDate": (datetime.now(timezone.utc) + timedelta(days=5)).isoformat()}
    r = requests.put(f"{BASE_URL}/{res_id}", json=payload, headers=other_headers)
    
    assert r.status_code == 404

def test_update_reservation_invalid_dates(reservations_url, auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    create_payload = {
        "licensePlate": "TEST-123",
        "parkingLotId": "11111111-1111-1111-1111-111111111111",
        "startDate": start,
        "endDate": end
    }
    res = requests.post(reservations_url, json=create_payload, headers=auth_headers)
    
    if res.status_code != 201:
        pytest.skip("Setup failed")
        
    res_id = res.json()["id"]
    
    payload = {
        "startDate": (datetime.now(timezone.utc) + timedelta(hours=5)).isoformat(),
        "endDate": (datetime.now(timezone.utc) + timedelta(hours=4)).isoformat()
    }
    
    r = requests.put(f"{reservations_url}/{res_id}", json=payload, headers=auth_headers)
    assert r.status_code == 400

def test_delete_reservation_success(reservations_url, auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    create_payload = {
        "licensePlate": "TEST-123",
        "parkingLotId": "11111111-1111-1111-1111-111111111111",
        "startDate": start,
        "endDate": end
    }
    res = requests.post(reservations_url, json=create_payload, headers=auth_headers)
    
    if res.status_code != 201:
        pytest.skip("Setup failed")
        
    res_id = res.json()["id"]
    
    r = requests.delete(f"{reservations_url}/{res_id}", headers=auth_headers)
    
    assert r.status_code == 204
    
    r_check = requests.get(reservations_url, headers=auth_headers)
    ids = [item["id"] for item in r_check.json()]
    assert res_id not in ids

def test_delete_reservation_other_user(reservations_url, other_headers, auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()
    
    create_payload = {
        "licensePlate": "TEST-123",
        "parkingLotId": "11111111-1111-1111-1111-111111111111",
        "startDate": start,
        "endDate": end
    }
    res = requests.post(reservations_url, json=create_payload, headers=auth_headers)
    
    if res.status_code != 201:
        pytest.skip("Setup failed")
        
    res_id = res.json()["id"]
    
    r = requests.delete(f"{reservations_url}/{res_id}", headers=other_headers)
    
    assert r.status_code == 404