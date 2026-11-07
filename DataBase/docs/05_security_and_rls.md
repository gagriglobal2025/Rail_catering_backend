# 05. Security & Authentication Architecture

---

## 1. Authentication & Role-Based Access Control (RBAC)

All internal operations are guarded by JWT-based authentication in `railcat_users`.

### Roles & System Capabilities:
| Role | Permitted Routes & Capabilities |
| :--- | :--- |
| `ADMIN` | Full access: `/dashboard`, `/terminal`, `/employees`, `/menu`, `/pricing`, `/users`, `/reports`, `/settings` |
| `COUNTER_OPERATOR` | POS order placement: `/terminal`, `/counter`, order status lookup, payment acceptance |
| `KITCHEN_STAFF` | Kitchen Display System: `/kitchen`, live preparation timers, order ready marking |

---

## 2. Concession Fraud Prevention & Device Locking

To prevent employees from sharing their 20% discount with non-railway customers:
1. **UMID Identification**: Railway Employee ID (`SCR-1042`, `G084`) is verified with an uploaded UMID ID card photograph.
2. **Device Hardware Fingerprint (`device_token`)**: The mobile ordering client generates a device signature (`dvt_redmi_24094rad4i_88f9c2`).
3. **One Employee = One Device**: When `device_locked = true`, any attempt to order with the same Employee ID from another device is blocked.
