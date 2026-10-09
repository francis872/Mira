# MIRA Repository Constitution

This document defines permanent repository rules. It does not contain version-specific domain requirements.

1. Specification before implementation.
2. The specification is the source of truth.
3. Controller → Service → Repository architecture.
4. Repository is the only layer authorized to access the database.
5. SQL must be parameterized.
6. Dependency Injection must be used.
7. SOLID principles must be followed.
8. Do not expose secrets.
9. Apply YAGNI.
10. Do not implement future versions early.
11. Each version has an independent Spec Kit.
12. Changes to functionality must first update the corresponding specification.
13. A version is validated before being tagged/closed.
14. The environment should be runnable with a simple documented command.
15. Authentication and authorization (login, tokens, passwords, roles, session-based restrictions) are out of scope for V1 and V2 and are reserved for a future version. General security measures (input validation, parameterized SQL, database integrity, CSRF token on forms) still apply.
16. Master-detail writes execute atomically in a single PostgreSQL routine; the API never splits them into independent writes.
