import pytest
import requests
from datetime import datetime, timedelta, timezone
import os


@pytest.fixture
def _data():
    return {
        "base": os.environ["BASE_URL"],
        "parkingLotId": os.environ.get("PARKING_LOT_ID", "11111111-1111-1111-1111-111111111111"),
        "licensePlate": os.environ.get("LICENSE_PLATE", "TEST123"),
        "users": {
            "user_a": {
                "email": os.environ["USER_A_EMAIL"],
                "password": os.environ["USER_A_PASSWORD"],
                "username": os.environ["USER_A_USERNAME"],
                "name": os.environ["USER_A_NAME"],
                "role": "user"
            },
            "user_b": {
                "email": os.environ["USER_B_EMAIL"],
                "password": os.environ["USER_B_PASSWORD"],
                "username": os.environ["USER_B_USERNAME"],
                "name": os.environ["USER_B_NAME"],
                "role": "user"
            },
            "admin": {
                "email": os.environ["ADMIN_EMAIL"],
                "password": os.environ["ADMIN_PASSWORD"],
                "username": os.environ["ADMIN_USERNAME"],
                "name": os.environ["ADMIN_NAME"],
                "role": "admin"
            }
        }
    }

def register_and_login(base_url, user):
    requests.post(f"{base_url}/register", json=user)

    r = requests.post(
        f"{base_url}/login",
        json={"username": user["username"], "password": user["password"]},
    )

    if r.status_code != 200 or "accesstoken" not in r.json():
        pytest.fail(
            f"Fout bij inloggen: {r.status_code} - {r.text} {r.json()}"
        )

    body = r.json()
    return f"Bearer {body['accesstoken']}" 

@pytest.fixture
def auth_headers(_data):
    return {"Authorization": register_and_login(_data["base"], _data["users"]["user_a"])}


@pytest.fixture
def other_headers(_data):
    return {"Authorization": register_and_login(_data["base"], _data["users"]["admin"])}


@pytest.fixture
def reservations_url(_data):
    return f"{_data['base']}/reservations"

@pytest.fixture(autouse=True)
def setup_data(_data, auth_headers):
    base = _data["base"]
    plate = _data["licensePlate"]

    veh_payload = {
        "licensePlate": plate,
        "make": "TestMake",
        "model": "TestModel",
        "color": "Red",
        "year": 2020
    }
    requests.post(f"{base}/vehicles", json=veh_payload, headers=auth_headers)

def test_create_reservation_vehicle_not_found(reservations_url, auth_headers, _data):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()

    payload = {
        "licensePlate": "NONEXISTENT",
        "parkingLotId": _data["parkingLotId"],
        "startDate": start,
        "endDate": end
    }
    r = requests.post(reservations_url, json=payload, headers=auth_headers)
    assert r.status_code == 400

def test_get_reservations_unauthorized(reservations_url):
    r = requests.get(reservations_url)
    assert r.status_code == 401

def test_update_reservation_success(reservations_url, auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()

    pl_resp = requests.post(f"{_data['base']}/parkinglots", json={
        "name": "Res Lot Update", "location": "Loc", "address": "Addr",
        "capacity": 50, "tariff": 1.0, "dayTariff": 5.0,
        "latitude": 0, "longitude": 0
    })
    pl_id = pl_resp.json().get("id") if pl_resp.status_code == 201 else "11111111-1111-1111-1111-111111111111"

    create_payload = {
        "licensePlate": "TEST123",
        "parkingLotId": pl_id,
        "startDate": start,
        "endDate": end
    }
    res = requests.post(reservations_url, json=create_payload, headers=auth_headers)
    if res.status_code != 201:
        pytest.skip("Kon geen reservering aanmaken")

    res_id = res.json()["id"]

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

    create_payload = {
        "licensePlate": _data["licensePlate"],
        "parkingLotId": _data["parkingLotId"],
        "startDate": start,
        "endDate": end
    }
    res = requests.post(reservations_url, json=create_payload, headers=auth_headers)
    if res.status_code != 201:
        pytest.skip("Setup failed")

    res_id = res.json()["id"]

    payload = {"startDate": (datetime.now(timezone.utc) + timedelta(days=5)).isoformat()}
    r = requests.put(f"{reservations_url}/{res_id}", json=payload, headers=other_headers)
    assert r.status_code == 404

def test_update_reservation_invalid_dates(reservations_url, auth_headers):
    start = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
    end = (datetime.now(timezone.utc) + timedelta(days=1, hours=2)).isoformat()

    create_payload = {
        "licensePlate": _data["licensePlate"],
        "parkingLotId": _data["parkingLotId"],
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
        "licensePlate": _data["licensePlate"],
        "parkingLotId": _data["parkingLotId"],
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
        "licensePlate": _data["licensePlate"],
        "parkingLotId": _data["parkingLotId"],
        "startDate": start,
        "endDate": end
    }
    res = requests.post(reservations_url, json=create_payload, headers=auth_headers)
    if res.status_code != 201:
        pytest.skip("Setup failed")

    res_id = res.json()["id"]

    r = requests.delete(f"{reservations_url}/{res_id}", headers=other_headers)
    assert r.status_code == 404