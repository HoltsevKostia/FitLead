# Invitations Feature Test Specification

## Purpose
This document defines test scope and test design for the Invitations feature in FitLead.

It is used to:
- define test conditions for invitation behavior
- define test cases and priorities
- align implementation of automated integration tests

## Feature Summary
Invitations feature responsibilities:
- trainer creates one-time invite links
- invite token is stored only as hash
- invite preview is publicly accessible
- authenticated client accepts invite by token
- accept creates trainer-client relationship
- trainer can revoke pending invite

This feature is business-critical because it is the entry point to trainer-client onboarding.

## Scope
In scope:
- invitation creation flow
- public invitation preview
- invitation acceptance flow
- trainer-client relationship creation
- revoke flow
- authorization and role enforcement for invitation endpoints
- persistence consistency after state-changing operations

Out of scope (this phase):
- frontend invitation UI behavior
- browser-level E2E
- messenger / outbox integration
- CSRF protection verification

## Test Level
Primary level: integration testing.

Rationale:
- invitation behavior crosses API + auth + persistence + domain orchestration boundaries
- correctness depends on interaction between token hashing, auth, state transitions, and database writes

## Test Conditions
- INV-COND-01: trainer can create one-time invite link with valid expiry
- INV-COND-02: invalid create request is rejected
- INV-COND-03: create stores token hash, not raw token
- INV-COND-04: preview is publicly accessible and exposes only public trainer data
- INV-COND-05: preview returns correct non-joinable states for invalid/accepted/expired/revoked invitations
- INV-COND-06: authenticated client can accept valid invite
- INV-COND-07: accept is idempotent for the same client and does not duplicate relationship
- INV-COND-08: accept is rejected for invalid token / wrong role / unauthorized access
- INV-COND-09: accept is rejected when client already has another active trainer
- INV-COND-10: trainer can revoke pending invitation

## Test Cases
### Planned / Implemented (Phase 1)
- INV-TC-01: Create valid 7-day invite -> `201` + invite URL returned + token hash persisted
- INV-TC-02: Create invalid expiry days -> `400` + validation problem details
- INV-TC-03: Create as client -> `403`
- INV-TC-04: Preview valid pending invite anonymously -> `200`, joinable, trainer full name visible
- INV-TC-05: Preview invalid token anonymously -> `404`
- INV-TC-06: Preview accepted invite -> `200`, `Accepted`, non-joinable
- INV-TC-07: Accept valid invite as authenticated client -> `204`, invitation accepted, relationship created
- INV-TC-08: Accept same invite twice by same client -> `204`, no duplicate relationship
- INV-TC-09: Accept invite while client already has another trainer -> `409`
- INV-TC-10: Revoke pending invite as owner trainer -> `204`, preview becomes `Revoked`

## Priority
Highest priority in this feature:
- token safety
- invitation state transition correctness
- authorization and role enforcement
- trainer-client relationship consistency

## Expected Artifacts
- stable invitation integration test suite
- reproducible execution locally and in CI
- traceability between conditions, cases, and automated tests

## References
- [Policy](/docs/testing/policy.md)
- [Master plan](/docs/testing/master-test-plan.md)
- [ADR-0015](/docs/adr/adr-0015-integration-first-testing-strategy.md)
- [Invitations controller](/FitLead/FitLead.Api/Invitations/InvitationsController.cs)
