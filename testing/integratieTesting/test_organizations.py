import uuid
import requests

BASE_URL = "http://localhost:8000"

ADMIN_USERNAME = "elemenefata"
ADMIN_PASSWORD = "Password123."


def get_admin_token():
    url = f"{BASE_URL}/auth/login"
    payload = {
        "username": ADMIN_USERNAME,
        "password": ADMIN_PASSWORD
    }
    resp = requests.post(url, json=payload)
    assert resp.status_code == 200, f"Login failed: {resp.status_code} {resp.text}"
    data = resp.json()
    token = data.get("token") or data.get("accessToken") or data.get("access_token")
    assert token, f"No token found in response: {data}"
    return token


def get_auth_headers():
    token = get_admin_token()
    return {"Authorization": f"Bearer {token}"}


def test_create_organization_success():
    url = f"{BASE_URL}/organizations"
    headers = get_auth_headers()

    unique_name = f"Test Org {uuid.uuid4()}"
    payload = {
        "name": unique_name,
        "address": "Teststraat 1",
        "contactEmail": "org@test.nl",
        "contactPhone": "0612345678"
    }

    resp = requests.post(url, json=payload, headers=headers)
    assert resp.status_code == 201, f"Expected 201, got {resp.status_code}, body={resp.text}"

    data = resp.json()
    org = data.get("organization") or data.get("data") or data
    assert org is not None
    assert org.get("name") == unique_name


def test_get_organizations_list():
    url = f"{BASE_URL}/organizations"
    headers = get_auth_headers()

    resp = requests.get(url, headers=headers)
    assert resp.status_code == 200, f"Expected 200, got {resp.status_code}, body={resp.text}"

    data = resp.json()
    organizations = data.get("organizations") or data.get("data") or []
    assert isinstance(organizations, list)


def test_get_organization_details():
    headers = get_auth_headers()

    create_url = f"{BASE_URL}/organizations"
    unique_name = f"Detail Org {uuid.uuid4()}"
    create_payload = {
        "name": unique_name,
        "address": "Detailstraat 5",
        "contactEmail": "detail@test.nl",
        "contactPhone": "0612345678"
    }

    create_resp = requests.post(create_url, json=create_payload, headers=headers)
    assert create_resp.status_code == 201, f"Create failed: {create_resp.status_code} {create_resp.text}"
    created_data = create_resp.json()
    org = created_data.get("organization") or created_data.get("data") or created_data
    org_id = org.get("id") or org.get("ID")
    assert org_id is not None


    get_url = f"{BASE_URL}/organizations/{org_id}"
    get_resp = requests.get(get_url, headers=headers)
    assert get_resp.status_code == 200, f"Expected 200, got {get_resp.status_code}, body={get_resp.text}"

    detail = get_resp.json().get("organization") or get_resp.json()
    assert detail.get("name") == unique_name


def test_update_organization():
    headers = get_auth_headers()

    create_url = f"{BASE_URL}/organizations"
    unique_name = f"Update Org {uuid.uuid4()}"
    create_payload = {
        "name": unique_name,
        "address": "Oldstraat 1",
        "contactEmail": "old@test.nl",
        "contactPhone": "0612345678"
    }

    create_resp = requests.post(create_url, json=create_payload, headers=headers)
    assert create_resp.status_code == 201
    org = create_resp.json().get("organization") or create_resp.json()
    org_id = org.get("id") or org.get("ID")


    update_url = f"{BASE_URL}/organizations/{org_id}"
    update_payload = {
        "name": unique_name + " NEW",
        "address": "Newstraat 99",
        "contactEmail": "new@test.nl",
        "contactPhone": "0699999999"
    }

    update_resp = requests.put(update_url, json=update_payload, headers=headers)
    assert update_resp.status_code == 200, f"Expected 200, got {update_resp.status_code}, body={update_resp.text}"

    updated = update_resp.json().get("organization") or update_resp.json()
    assert updated.get("name") == update_payload["name"]
    assert updated.get("address") == update_payload["address"]


def test_delete_organization_without_relations():
    headers = get_auth_headers()


    create_url = f"{BASE_URL}/organizations"
    unique_name = f"Delete Org {uuid.uuid4()}"
    create_payload = {
        "name": unique_name,
        "address": "Deletelaan 1",
        "contactEmail": "delete@test.nl",
        "contactPhone": "0612345678"
    }

    create_resp = requests.post(create_url, json=create_payload, headers=headers)
    assert create_resp.status_code == 201
    org = create_resp.json().get("organization") or create_resp.json()
    org_id = org.get("id") or org.get("ID")


    delete_url = f"{BASE_URL}/organizations/{org_id}"
    delete_resp = requests.delete(delete_url, headers=headers)
    assert delete_resp.status_code in (200, 204), f"Expected 200/204, got {delete_resp.status_code}, body={delete_resp.text}"


    get_url = f"{BASE_URL}/organizations/{org_id}"
    get_resp = requests.get(get_url, headers=headers)
    assert get_resp.status_code == 404
