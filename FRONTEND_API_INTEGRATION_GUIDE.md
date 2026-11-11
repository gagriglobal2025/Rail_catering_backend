# RailCat Platform — Frontend UI to Backend API Integration Guide

This guide provides a comprehensive mapping between the Frontend UI screens/components (for both the **Operations Web Dashboard** and the **Customer/Employee Mobile Web App**) and the backend microservice APIs.

---

## Executive Summary & Integration Highlights

This integration specification binds the two frontend applications (`RailwayCatering_Mobile-web` and `WEB` Operations Dashboard) with the four core ASP.NET Core 8 microservices hosted on IIS (`https://coreapiv1catering.gagriglobal.com/`).

### Key Capabilities & Architectural Guarantees

1. **Auto-Generated Primary Keys Across All Entities**:
   * **Orders**: `OrderId` (UUID), `OrderNumber` (`ORD-YYYYMMDD-XXXX`), and `QueueToken` (`A-001`, `A-002` resetting daily at midnight IST) are 100% server-generated. Clients never supply primary keys.
   * **Catalog**: `ItemId` is automatically slugified from `itemName` (e.g. `ghee-podi-idli-8a21`) with collision-resistant suffixes.
   * **Settlement**: `ShiftId` (UUID) and `PaymentId` (UUID) are generated automatically upon opening shifts or recording payments.

2. **Zero-Configuration Image Upload & Rendering**:
   * **Direct System Upload**: Supports system image uploads up to **10 MB** (`multipart/form-data`) or external image URLs (`application/json`).
   * **Auto-Prepend Absolute URLs**: Whenever an image is uploaded and stored in `wwwroot/uploads/dishes/`, the API resolves and returns the **full absolute URL** (e.g., `https://coreapiv1catering.gagriglobal.com/catalog/uploads/dishes/...`). Frontends can directly bind `<img src={item.imageUrl} />` with **zero URL stitching or custom domain concatenation**.

3. **Flexible Authentication (With / Without `Bearer ` Prefix)**:
   * Frontends can pass the JWT token as `Authorization: Bearer <token>` or simply `Authorization: <token>`. Both formats are supported across all protected endpoints.

4. **Partial Updates (Safe Coalescing)**:
   * All update endpoints (`PUT /api/menu-items/{id}`, `PATCH /api/menu-items/{id}/availability`, etc.) treat omitted or null fields as unedited, preserving existing database values rather than overwriting them to null.

5. **Integrated Peripheral & Thermal Printing**:
   * When an order is marked `PAID` at the counter, the system automatically generates Kitchen Order Tickets (KOT) and Cash Receipts into the `print_spool` queue ready for background hardware agents.

---

### Frontend Screens to Backend Services Quick Reference

| Frontend Application | Screen / Component | Microservice | Primary Endpoints |
| :--- | :--- | :--- | :--- |
| **Mobile Web** | Home & Menu Browse (`HomeScreen.tsx`, `MenuScreen.tsx`) | `Catalog.Api` | `GET /api/categories`<br/>`GET /api/menu-items` |
| **Mobile Web** | Railway Employee Verification (`EmployeeSheet.tsx`) | `Catalog.Api` | `POST /api/employees/verify` |
| **Mobile Web** | Cart & Checkout (`CheckoutSheet.tsx`) | `Orders.Api` | `POST /api/orders` |
| **Mobile Web** | Live Order Token Tracker (`OrdersScreen.tsx`) | `Orders.Api` | `GET /api/orders/by-number/{orderNumber}` |
| **Web Dashboard** | Staff Login & Token Refresh (`login/page.tsx`) | `Users.Api` | `POST /api/auth/login`<br/>`POST /api/auth/refresh`<br/>`POST /api/auth/logout` |
| **Web Dashboard** | POS Terminal & Shifts (`counter/page.tsx`, `terminal/page.tsx`) | `Settlement.Api`<br/>`Orders.Api` | `GET /api/shifts/active`<br/>`POST /api/shifts/open`<br/>`PATCH /api/orders/{id}/payment`<br/>`POST /api/shifts/{id}/close` |
| **Web Dashboard** | Kitchen KDS Display (`kitchen/page.tsx`) | `Orders.Api` | `GET /api/kitchen/tickets`<br/>`PATCH /api/kitchen/tickets/{orderId}/status` |
| **Web Dashboard** | Dish & Catalog Management (`menu/page.tsx`) | `Catalog.Api` | `POST /api/menu-items` (10MB upload)<br/>`PUT /api/menu-items/{id}`<br/>`PATCH /api/menu-items/{id}/availability` |
| **Web Dashboard** | Reports & Revenue (`reports/page.tsx`) | `Settlement.Api` | `GET /api/reports/daily-sales` |
| **Hardware Agent** | Thermal Receipt & KOT Printers | `Orders.Api` | `GET /api/print-spool/pending`<br/>`PATCH /api/print-spool/{jobId}/printed` |

---

## 1. Architecture & Service Base URLs

| Microservice | Local Development URL | IIS Live Production Base URL | Swagger UI URL |
| :--- | :--- | :--- | :--- |
| **RailCat.Users.Api** | `http://localhost:5001` | `https://coreapiv1catering.gagriglobal.com/users/` | `/users/swagger` |
| **RailCat.Catalog.Api** | `http://localhost:5002` | `https://coreapiv1catering.gagriglobal.com/catalog/` | `/catalog/swagger` |
| **RailCat.Orders.Api** | `http://localhost:5003` | `https://coreapiv1catering.gagriglobal.com/orders/` | `/orders/swagger` |
| **RailCat.Settlement.Api** | `http://localhost:5004` | `https://coreapiv1catering.gagriglobal.com/settlement/` | `/settlement/swagger` |

### Global Authentication Standard
All protected endpoints accept the JWT token in the `Authorization` HTTP header. The backend transparently accepts tokens **with or without** the `Bearer ` prefix:
```http
Authorization: Bearer <jwt-token>
-- OR --
Authorization: <jwt-token>
```

---

## 2. Customer Mobile Web App (`RailwayCatering_Mobile-web`)

### Screen 1: Home & Menu Catalog Screen
* **Frontend Files**: `src/features/home/HomeScreen.tsx`, `src/features/menu/MenuScreen.tsx`, `src/features/menu/MenuItemCard.tsx`
* **Purpose**: Displays categories filter pills, food dishes with price, veg/non-veg badges, and dish photos.

#### A. Fetch Food Categories
* **Endpoint**: `GET /api/categories`
* **Service**: `RailCat.Catalog.Api`
* **Auth**: None (Public)
* **cURL**:
  ```bash
  curl -X GET "https://coreapiv1catering.gagriglobal.com/catalog/api/categories"
  ```
* **Response (200 OK)**:
  ```json
  [
    {
      "categoryId": "breakfast",
      "categoryName": "Breakfast",
      "sortOrder": 1,
      "isActive": true
    },
    {
      "categoryId": "lunch",
      "categoryName": "Lunch & Meals",
      "sortOrder": 2,
      "isActive": true
    }
  ]
  ```

#### B. Fetch Menu Items (with full dish image URL)
* **Endpoint**: `GET /api/menu-items?categoryId={categoryId}`
* **Service**: `RailCat.Catalog.Api`
* **Auth**: None (Public)
* **cURL**:
  ```bash
  curl -X GET "https://coreapiv1catering.gagriglobal.com/catalog/api/menu-items?categoryId=breakfast"
  ```
* **Response (200 OK)**:
  ```json
  [
    {
      "itemId": "masala-dosa-3f8a",
      "categoryId": "breakfast",
      "itemName": "Masala Dosa",
      "itemDescription": "Crispy golden crepe with spiced potato filling, coconut chutney and sambar",
      "basePrice": 60.00,
      "emojiIcon": "🥞",
      "imageUrl": "https://coreapiv1catering.gagriglobal.com/catalog/uploads/dishes/masala-dosa_3f8a12bc.png",
      "isVegetarian": true,
      "isAvailable": true,
      "isSoldOut": false,
      "isBestseller": true,
      "isSpicy": false,
      "isKidsChoice": false,
      "rating": 4.8,
      "ratingCount": 142,
      "prepTime": "10-12 mins"
    }
  ]
  ```
* **UI Binding**:
  * Pass `item.imageUrl` directly to `<img src={item.imageUrl} />`. It is already fully qualified.
  * `isSoldOut === true` &rarr; Disables the "ADD" button and displays "Next batch soon" badge.

---

### Screen 2: Railway Employee Concession Verification Sheet
* **Frontend File**: `src/features/employee/EmployeeSheet.tsx`
* **Purpose**: Railway staff enter their Employee ID / PPO Number and OTP/PIN to claim concession quota.

* **Endpoint**: `POST /api/employees/verify`
* **Service**: `RailCat.Catalog.Api`
* **Auth**: None (Public verification)
* **Request Payload**:
  ```json
  {
    "employeeId": "SCR-104928",
    "verificationPin": "1234",
    "deviceToken": "web-browser-device-uuid-9988"
  }
  ```
* **cURL**:
  ```bash
  curl -X POST "https://coreapiv1catering.gagriglobal.com/catalog/api/employees/verify" \
    -H "Content-Type: application/json" \
    -d '{
      "employeeId": "SCR-104928",
      "verificationPin": "1234",
      "deviceToken": "web-browser-device-uuid-9988"
    }'
  ```
* **Response (200 OK - Verified)**:
  ```json
  {
    "isVerified": true,
    "employeeId": "SCR-104928",
    "fullName": "K. V. Raman",
    "designation": "Senior Loco Pilot",
    "department": "Mechanical / Operations",
    "stationId": "SC",
    "discountPercentage": 30.0,
    "dailyQuotaRemaining": 2,
    "message": "Employee verified successfully. 30% concession applied."
  }
  ```
* **Response (400 / 403 - Blocked / Exceeded Quota / Device Lock Violation)**:
  ```json
  {
    "isVerified": false,
    "message": "Device token mismatch. This employee profile is locked to another authorized handset."
  }
  ```

---

### Screen 3: Cart & Checkout Screen
* **Frontend Files**: `src/features/checkout/CheckoutSheet.tsx`, `src/features/cart/CartScreen.tsx`
* **Purpose**: Submits the customer order, calculates concession & 5% GST, generates queue token.

* **Endpoint**: `POST /api/orders`
* **Service**: `RailCat.Orders.Api`
* **Auth**: Public or Counter Operator Bearer
* **Request Payload**:
  ```json
  {
    "customerType": "NORMAL",
    "customerRef": "9876543210",
    "pickupCounter": "Counter 1",
    "specialInstructions": "Less spicy, extra chutney",
    "items": [
      {
        "itemId": "masala-dosa-3f8a",
        "itemName": "Masala Dosa",
        "quantity": 2,
        "unitPrice": 60.00
      },
      {
        "itemId": "filter-coffee-01",
        "itemName": "South Indian Filter Coffee",
        "quantity": 2,
        "unitPrice": 25.00
      }
    ]
  }
  ```
* **cURL**:
  ```bash
  curl -X POST "https://coreapiv1catering.gagriglobal.com/orders/api/orders" \
    -H "Content-Type: application/json" \
    -d '{
      "customerType": "NORMAL",
      "customerRef": "9876543210",
      "pickupCounter": "Counter 1",
      "specialInstructions": "Less spicy",
      "items": [
        {
          "itemId": "masala-dosa-3f8a",
          "itemName": "Masala Dosa",
          "quantity": 2,
          "unitPrice": 60.00
        },
        {
          "itemId": "filter-coffee-01",
          "itemName": "South Indian Filter Coffee",
          "quantity": 2,
          "unitPrice": 25.00
        }
      ]
    }'
  ```
* **Response (201 Created)**:
  ```json
  {
    "orderId": "73e970cb-e45f-4a0f-ba26-805175659858",
    "orderNumber": "ORD-20261110-B14C",
    "queueToken": "A-014",
    "customerType": "NORMAL",
    "customerRef": "9876543210",
    "pickupCounter": "Counter 1",
    "subtotalAmount": 170.00,
    "concessionDiscount": 0.00,
    "taxAmount": 8.50,
    "grandTotal": 178.50,
    "orderStatus": "NEW",
    "paymentStatus": "PENDING",
    "paymentMethod": null,
    "placedAt": "2026-11-10T11:45:00.000Z",
    "paidAt": null,
    "preparingAt": null,
    "readyAt": null,
    "deliveredAt": null,
    "items": [
      {
        "orderItemId": 101,
        "itemId": "masala-dosa-3f8a",
        "itemName": "Masala Dosa",
        "quantity": 2,
        "unitPrice": 60.00,
        "lineTotal": 120.00
      },
      {
        "orderItemId": 102,
        "itemId": "filter-coffee-01",
        "itemName": "South Indian Filter Coffee",
        "quantity": 2,
        "unitPrice": 25.00,
        "lineTotal": 50.00
      }
    ]
  }
  ```
* **UI Binding**:
  * Store `orderNumber` and `orderId` in local storage for the Live Order Tracker.
  * Display `queueToken` (`A-014`) prominently on the confirmation screen.

---

### Screen 4: Live Order Status & Token Tracker
* **Frontend File**: `src/features/orders/OrdersScreen.tsx`
* **Purpose**: Live tracking of token status (`NEW` &rarr; `PREPARING` &rarr; `READY` &rarr; `DELIVERED`).

* **Endpoint**: `GET /api/orders/by-number/{orderNumber}`
* **Service**: `RailCat.Orders.Api`
* **Auth**: Public / Client Token
* **cURL**:
  ```bash
  curl -X GET "https://coreapiv1catering.gagriglobal.com/orders/api/orders/by-number/ORD-20261110-B14C"
  ```
* **Response (200 OK)**:
  ```json
  {
    "orderId": "73e970cb-e45f-4a0f-ba26-805175659858",
    "orderNumber": "ORD-20261110-B14C",
    "queueToken": "A-014",
    "orderStatus": "READY",
    "paymentStatus": "PAID",
    "placedAt": "2026-11-10T11:45:00.000Z",
    "preparingAt": "2026-11-10T11:47:30.000Z",
    "readyAt": "2026-11-10T11:54:12.000Z",
    "deliveredAt": null
  }
  ```
* **UI Step Mapping**:
  * `NEW` &rarr; Step 1: "Order Placed, Waiting for Kitchen"
  * `PREPARING` &rarr; Step 2: "In the Kitchen"
  * `READY` &rarr; Step 3: "Ready for Pickup at Counter 1" (Triggers sound / animation)
  * `DELIVERED` &rarr; Step 4: "Delivered / Completed"

---

## 3. Operations Web Dashboard (`WEB`)

### Screen 1: Staff Login & Token Refresh
* **Frontend File**: `src/app/login/page.tsx`
* **Purpose**: Authenticates Counter Operators, Kitchen Staff, Store Managers, and Admins; maintains sessions using JWT Access & Refresh Tokens.

#### A. Staff Login (Returns Access Token + 7-Day Refresh Token)
* **Endpoint**: `POST /api/auth/login`
* **Service**: `RailCat.Users.Api`
* **Auth**: None
* **Request Payload**:
  ```json
  {
    "employeeIdOrEmail": "cashier@railcat.local",
    "password": "Password123!"
  }
  ```
* **cURL**:
  ```bash
  curl -X POST "https://coreapiv1catering.gagriglobal.com/users/api/auth/login" \
    -H "Content-Type: application/json" \
    -d '{
      "employeeIdOrEmail": "cashier@railcat.local",
      "password": "Password123!"
    }'
  ```
* **Response (200 OK)**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-11-11T13:00:00Z",
    "refreshToken": "3cvkQVLsi-P2jOiDZGr8BGn_z1gWn9_4...",
    "refreshTokenExpiresAt": "2026-11-18T05:00:00Z",
    "userId": "a0000000-0000-0000-0000-000000000001",
    "employeeId": "EMP-001",
    "fullName": "Suresh Kumar",
    "email": "cashier@railcat.local",
    "role": "COUNTER_OPERATOR",
    "stationId": "SC"
  }
  ```
* **UI Action**:
  * Store `token` (access token) and `refreshToken` in `localStorage` or secure auth cookies.
  * Route by role:
    * `COUNTER_OPERATOR` &rarr; `/counter`
    * `KITCHEN_STAFF` &rarr; `/kitchen`
    * `STORE_MANAGER` / `ADMIN` &rarr; `/dashboard`

#### B. Silent Token Refresh (OWASP Refresh Token Rotation)
* **Endpoint**: `POST /api/auth/refresh`
* **Service**: `RailCat.Users.Api`
* **Auth**: None
* **Request Payload**:
  ```json
  {
    "refreshToken": "3cvkQVLsi-P2jOiDZGr8BGn_z1gWn9_4..."
  }
  ```
* **cURL**:
  ```bash
  curl -X POST "https://coreapiv1catering.gagriglobal.com/users/api/auth/refresh" \
    -H "Content-Type: application/json" \
    -d '{
      "refreshToken": "3cvkQVLsi-P2jOiDZGr8BGn_z1gWn9_4..."
    }'
  ```
* **Response (200 OK)**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.new...",
    "expiresAt": "2026-11-11T21:00:00Z",
    "refreshToken": "fPqSZ7LfHIJY5JNmP8QwqWEB-new...",
    "refreshTokenExpiresAt": "2026-11-18T13:00:00Z"
  }
  ```
* **Frontend Axios / Fetch Interceptor**:
  * When any API request fails with a `401 Unauthorized`, intercept the response, call `/api/auth/refresh` with the stored `refreshToken`, update the stored tokens, and retry the failed request automatically.

#### C. Staff Logout (Session Invalidation & Token Revocation)
* **Endpoint**: `POST /api/auth/logout`
* **Service**: `RailCat.Users.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Request Payload (Optional)**:
  ```json
  {
    "refreshToken": "fPqSZ7LfHIJY5JNmP8QwqWEB-new..."
  }
  ```
* **cURL**:
  ```bash
  curl -X POST "https://coreapiv1catering.gagriglobal.com/users/api/auth/logout" \
    -H "Authorization: Bearer <token>" \
    -H "Content-Type: application/json" \
    -d '{ "refreshToken": "fPqSZ7LfHIJY5JNmP8QwqWEB-new..." }'
  ```
* **Response (200 OK)**:
  ```json
  { "message": "Logged out successfully." }
  ```

---

### Screen 2: Counter POS Terminal & Shift Register
* **Frontend Files**: `src/app/counter/page.tsx`, `src/app/terminal/page.tsx`
* **Roles**: `COUNTER_OPERATOR`, `STORE_MANAGER`, `ADMIN`

#### A. Check If Active Shift is Already Open
* **Endpoint**: `GET /api/shifts/active?cashierId={userId}`
* **Service**: `RailCat.Settlement.Api`
* **Auth**: `Authorization: Bearer <token>`
* **cURL**:
  ```bash
  curl -X GET "https://coreapiv1catering.gagriglobal.com/settlement/api/shifts/active" \
    -H "Authorization: Bearer <token>"
  ```
* **Response (200 OK - Active Shift Exists)**:
  ```json
  {
    "shiftId": "f128bcde-3321-4f11-9a99-90123456789a",
    "cashierUserId": "a0000000-0000-0000-0000-000000000001",
    "startingCash": 500.00,
    "cashCollected": 1240.00,
    "upiCollected": 3520.00,
    "cardCollected": 0.00,
    "totalExpected": 1740.00,
    "shiftStatus": "OPEN",
    "openedAt": "2026-11-10T08:00:00Z"
  }
  ```
* **Response (404 Not Found - Cashier must open shift first)**:
  ```json
  { "message": "No active open shift found." }
  ```

#### B. Open Cashier Shift (Start of Day Float)
* **Endpoint**: `POST /api/shifts/open`
* **Service**: `RailCat.Settlement.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Request Payload**:
  ```json
  {
    "cashierUserId": "a0000000-0000-0000-0000-000000000001",
    "startingCash": 500.00
  }
  ```
* **cURL**:
  ```bash
  curl -X POST "https://coreapiv1catering.gagriglobal.com/settlement/api/shifts/open" \
    -H "Authorization: Bearer <token>" \
    -H "Content-Type: application/json" \
    -d '{ "cashierUserId": "a0000000-0000-0000-0000-000000000001", "startingCash": 500.00 }'
  ```

#### C. Accept Payment for Order at Counter (Cash / Counter UPI)
* **Endpoint**: `PATCH /api/orders/{orderId}/payment`
* **Service**: `RailCat.Orders.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Effect**: Stamps order as `PAID`, populates `paidAt`, and **automatically spools KOT & Receipt thermal print jobs**.
* **Request Payload**:
  ```json
  {
    "paymentMethod": "CASH",
    "amount": 178.50,
    "gatewayReference": "CASH-COUNTER-DRAWER-1"
  }
  ```
* **cURL**:
  ```bash
  curl -X PATCH "https://coreapiv1catering.gagriglobal.com/orders/api/orders/73e970cb-e45f-4a0f-ba26-805175659858/payment" \
    -H "Authorization: Bearer <token>" \
    -H "Content-Type: application/json" \
    -d '{ "paymentMethod": "CASH", "amount": 178.50, "gatewayReference": "CASH-COUNTER-DRAWER-1" }'
  ```
* **Response (200 OK)**: Returns updated Order with `paymentStatus: "PAID"`.

#### D. Record Settlement Transaction into Cashier Drawer
* **Endpoint**: `POST /api/payments`
* **Service**: `RailCat.Settlement.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Request Payload**:
  ```json
  {
    "orderId": "73e970cb-e45f-4a0f-ba26-805175659858",
    "shiftId": "f128bcde-3321-4f11-9a99-90123456789a",
    "amount": 178.50,
    "paymentMethod": "CASH",
    "gatewayReference": "CASH-COUNTER-DRAWER-1"
  }
  ```
* **Response (200 OK)**: Records payment and automatically recalculates shift totals (`cashCollected`, `totalExpected`).

#### E. Close Shift (End of Day Handover)
* **Endpoint**: `POST /api/shifts/{shiftId}/close`
* **Service**: `RailCat.Settlement.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Request Payload**:
  ```json
  {
    "actualCashCounted": 1740.00
  }
  ```
* **Response (200 OK)**: Closes shift, records physical cash counted vs system expected, calculates variance.

---

### Screen 3: Kitchen Display System (KDS)
* **Frontend Files**: `src/app/kitchen/page.tsx`, `src/app/kitchen/new/page.tsx`, `src/app/kitchen/preparing/page.tsx`, `src/app/kitchen/ready/page.tsx`
* **Roles**: `KITCHEN_STAFF`, `STORE_MANAGER`, `ADMIN`

#### A. Fetch Live Kitchen Tickets
* **Endpoint**: `GET /api/kitchen/tickets`
* **Service**: `RailCat.Orders.Api`
* **Auth**: `Authorization: Bearer <token>`
* **cURL**:
  ```bash
  curl -X GET "https://coreapiv1catering.gagriglobal.com/orders/api/kitchen/tickets" \
    -H "Authorization: Bearer <token>"
  ```
* **Response (200 OK)**:
  ```json
  [
    {
      "orderId": "73e970cb-e45f-4a0f-ba26-805175659858",
      "orderNumber": "ORD-20261110-B14C",
      "queueToken": "A-014",
      "orderStatus": "NEW",
      "placedAt": "2026-11-10T11:45:00Z",
      "elapsedMinutes": 3,
      "specialInstructions": "Less spicy",
      "items": [
        {
          "itemName": "Masala Dosa",
          "quantity": 2
        },
        {
          "itemName": "South Indian Filter Coffee",
          "quantity": 2
        }
      ]
    }
  ]
  ```

#### B. Advance Ticket Status (Start Prep / Mark Ready)
* **Endpoint**: `PATCH /api/kitchen/tickets/{orderId}/status`
* **Service**: `RailCat.Orders.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Request Payload**:
  ```json
  {
    "nextStatus": "PREPARING"
  }
  ```
  *(Next status can be `"PREPARING"`, `"READY"`, or `"CANCELLED"`)*
* **cURL**:
  ```bash
  curl -X PATCH "https://coreapiv1catering.gagriglobal.com/orders/api/kitchen/tickets/73e970cb-e45f-4a0f-ba26-805175659858/status" \
    -H "Authorization: Bearer <token>" \
    -H "Content-Type: application/json" \
    -d '{ "nextStatus": "PREPARING" }'
  ```
* **Response (200 OK)**:
  ```json
  { "message": "Status updated to PREPARING" }
  ```

---

### Screen 4: Menu Items & Dishes Management
* **Frontend File**: `src/app/menu/page.tsx`
* **Roles**: `STORE_MANAGER`, `ADMIN`

#### A. Create New Dish with System File Upload (10 MB Limit)
* **Endpoint**: `POST /api/menu-items`
* **Service**: `RailCat.Catalog.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Content-Type**: `multipart/form-data`
* **Form Fields**:
  * `imageFile`: Image binary (`.png`, `.jpg`, `.jpeg`, `.webp` up to 10 MB)
  * `categoryId`: string (e.g. `breakfast`)
  * `itemName`: string (e.g. `Ghee Podi Idli`)
  * `itemDescription`: string
  * `basePrice`: number (e.g. `70.00`)
  * `emojiIcon`: string (e.g. `🥟`)
  * `isVegetarian`: boolean (`true`/`false`)
  * `isBestseller`: boolean (`true`/`false`)
  * `prepTime`: string (e.g. `8-10 mins`)
* **cURL**:
  ```bash
  curl -X POST "https://coreapiv1catering.gagriglobal.com/catalog/api/menu-items" \
    -H "Authorization: Bearer <token>" \
    -F "categoryId=breakfast" \
    -F "itemName=Ghee Podi Idli" \
    -F "itemDescription=Mini idlis tossed in gun powder and aromatic ghee" \
    -F "basePrice=70.00" \
    -F "emojiIcon=🥟" \
    -F "isVegetarian=true" \
    -F "isBestseller=true" \
    -F "prepTime=8-10 mins" \
    -F "imageFile=@C:/Users/Admin/Pictures/podi_idli.jpg"
  ```
* **Response (201 Created)**:
  ```json
  {
    "itemId": "ghee-podi-idli-8a21",
    "categoryId": "breakfast",
    "itemName": "Ghee Podi Idli",
    "basePrice": 70.00,
    "imageUrl": "https://coreapiv1catering.gagriglobal.com/catalog/uploads/dishes/ghee-podi-idli_8a2100de.jpg",
    "isAvailable": true,
    "isSoldOut": false
  }
  ```

#### B. Partial Update Dish (Omitted fields retain existing values)
* **Endpoint**: `PUT /api/menu-items/{id}`
* **Service**: `RailCat.Catalog.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Content-Type**: `multipart/form-data` or `application/json`
* **Payload (Example: Update only price and image)**:
  ```bash
  curl -X PUT "https://coreapiv1catering.gagriglobal.com/catalog/api/menu-items/ghee-podi-idli-8a21" \
    -H "Authorization: Bearer <token>" \
    -F "basePrice=75.00" \
    -F "imageFile=@C:/Users/Admin/Pictures/new_podi_idli.jpg"
  ```

#### C. Toggle Dish Sold-Out / Availability
* **Endpoint**: `PATCH /api/menu-items/{id}/availability`
* **Service**: `RailCat.Catalog.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Request Payload**:
  ```json
  {
    "isAvailable": true,
    "isSoldOut": true
  }
  ```
* **Response (200 OK)**:
  ```json
  { "message": "Menu item availability updated." }
  ```

#### D. Delete Dish
* **Endpoint**: `DELETE /api/menu-items/{id}`
* **Service**: `RailCat.Catalog.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Response (200 OK)**:
  ```json
  { "message": "Menu item deleted successfully." }
  ```

---

### Screen 5: Daily Revenue & Settlement Reports
* **Frontend File**: `src/app/reports/page.tsx`
* **Roles**: `STORE_MANAGER`, `ADMIN`

* **Endpoint**: `GET /api/reports/daily-sales?date=YYYY-MM-DD`
* **Service**: `RailCat.Settlement.Api`
* **Auth**: `Authorization: Bearer <token>`
* **cURL**:
  ```bash
  curl -X GET "https://coreapiv1catering.gagriglobal.com/settlement/api/reports/daily-sales?date=2026-11-10" \
    -H "Authorization: Bearer <token>"
  ```
* **Response (200 OK)**:
  ```json
  {
    "summaryDate": "2026-11-10",
    "totalOrders": 142,
    "grossSales": 28450.00,
    "concessionsGiven": 2420.00,
    "taxCollected": 1301.50,
    "netRevenue": 27331.50,
    "upiRevenue": 18200.00,
    "cashRevenue": 8131.50,
    "cardRevenue": 1000.00
  }
  ```

---

### Screen 6: Thermal Hardware Print Spooler (Agent / Peripheral)
* **Frontend / Agent Hook**: Peripheral print service polling
* **Roles**: All authorized staff

#### A. Fetch Pending Print Jobs (KOT & Receipts)
* **Endpoint**: `GET /api/print-spool/pending`
* **Service**: `RailCat.Orders.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Response (200 OK)**:
  ```json
  [
    {
      "jobId": "240a083c-7971-458e-91d9-57f5d210c361",
      "orderId": "73e970cb-e45f-4a0f-ba26-805175659858",
      "jobType": "KOT",
      "rawContent": "=== KITCHEN ORDER TICKET ===\nToken: A-014\nOrder: ORD-20261110-B14C\nTime: 11:45\n--------------------------\n2x Masala Dosa\n2x South Indian Filter Coffee\nNote: Less spicy\n==========================",
      "printStatus": "PENDING",
      "createdAt": "2026-11-10T11:45:01Z"
    }
  ]
  ```

#### B. Mark Job as Printed
* **Endpoint**: `PATCH /api/print-spool/{jobId}/printed`
* **Service**: `RailCat.Orders.Api`
* **Auth**: `Authorization: Bearer <token>`
* **Response (200 OK)**:
  ```json
  { "message": "Print job marked as PRINTED." }
  ```

---

## 4. Quick Field Reference & Validation Rules

| Field Name | Type | Rules / Behavior |
| :--- | :--- | :--- |
| `customerType` | `string` | Must be `"NORMAL"` or `"RAILWAY_EMPLOYEE"`. |
| `paymentMethod` | `string` | Must be `"CASH"`, `"UPI"`, or `"CARD"`. |
| `orderStatus` | `string` | Allowed lifecycle: `"NEW"` &rarr; `"PREPARING"` &rarr; `"READY"` &rarr; `"DELIVERED"` (or `"CANCELLED"`). |
| `paymentStatus` | `string` | `"PENDING"` &rarr; `"PAID"` (or `"FAILED"`). |
| `shiftStatus` | `string` | `"OPEN"` &rarr; `"CLOSED"`. |
| `imageFile` | `IFormFile` | Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.webp`. Max size: **10 MB**. |
| `imageUrl` | `string` | Direct full URL to uploaded file or external image link. Generated automatically on upload. |
| Primary IDs (`ItemId`, `OrderId`, `ShiftId`, etc.) | `string` / `UUID` | **Auto-generated by server**. Never send in creation requests. |

---

## 5. Standard Error Responses

When an error occurs, all microservices return structured JSON:
```json
{
  "message": "Specific error description",
  "errors": {
    "FieldName": [
      "Validation failure explanation"
    ]
  }
}
```

| Status Code | Meaning | Action Needed by Frontend UI |
| :--- | :--- | :--- |
| **400 Bad Request** | Validation failed (e.g. file > 10MB or missing items). | Display field error message to user. |
| **401 Unauthorized** | Missing or expired JWT token. | Redirect to `/login`. |
| **403 Forbidden** | Role not permitted (e.g. Cashier attempting to edit menu). | Display access denied banner. |
| **404 Not Found** | Record (Order/Shift/Dish) does not exist. | Display 404 state. |
| **409 Conflict** | Shift already open or concurrency conflict. | Prompt user to close previous shift first. |
| **500 Server Error** | Unhandled internal exception. | Logged to Serilog daily files (`logs/`). UI shows friendly retry toast. |

