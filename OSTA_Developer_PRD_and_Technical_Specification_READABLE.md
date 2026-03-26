OSTA
Developer PRD and Technical Specification
Reference document for product definition, domain rules, architecture, data model, coding
workflow, and Copilot alignment

Fabrication manufacturing control platform for
**System type:** project-based production

Convert engineering BOM data into production-

**Primary objective:** ready, trackable business transactions

**Core identity:** Project → FG → Assembly → Part → Revision

PostgreSQL + ASP.NET Core (.NET) + React +

**Recommended stack:** TypeScript

Use this document as the single source of truth when prompting Copilot, planning modules, or
reviewing code decisions.

## 1. Product Definition
OSTA is a fabrication-oriented manufacturing control platform built for project-based production.
It turns engineering BOM structures into production-ready, trackable business transactions that
connect materials, nesting, operations, assembly readiness, planning, and execution.
- The platform is not a generic ERP screen set, not a pure MRP calculator, and not a shop-floor
tracker in isolation. Its value comes from enforcing relationships and state transitions across all
of them.
- Primary business questions OSTA must answer:
- What should be made?
- What should be bought?
- What is missing?
- What is already nested or cut?
- Which assembly is ready for fit-up?
- Which project is delayed, where, and why?

## 2. Scope and Business Problem
In many fabrication factories, engineering BOMs, material purchasing, nesting reports, cutting
updates, assembly progress, and completion reporting live in disconnected tools. This causes weak
traceability, manual spreadsheet dependence, false shortage signals, late assemblies, and poor cost
visibility.
OSTA solves this by making the part the central controlled object and by calculating readiness and
shortages from transactions instead of assumptions.

Problem Area Current Failure Mode OSTA Response

Part identity breaks across Single controlled part identity
Traceability BOM, nest, and shop-floor with validation and
updates transactional linking

Demand explosion, material
Shortages are guessed from
Planning linkage, shortage detection,
partial files and manual sheets
and status-aware planning

Cutting and assembly progress Event-driven updates from
Execution are updated manually and nest imports and production
inconsistently transactions

Assemblies are assumed ready Calculated readiness rules
Readiness before required parts are based on part completion and
complete gating logic

## 3. Non-Negotiable Domain Rules
- The same part number must remain consistent across BOM, nesting, production tracking, and
assembly readiness logic.
- Demand always starts from Project, then FG or equipment, then Assembly, then Part, then
Revision.
- Engineering release must exist before production execution is allowed.
- Material availability and route definition are prerequisites for executable manufacturing
demand.
- Assembly fit-up cannot begin until all required parts for that assembly satisfy the readiness
rule.
- OSTA is state-based: user actions and external imports create transactions that change object
status over time.
- Derived values such as readiness, shortage, completion percentage, and delay reason are
calculated, not manually typed.

## 4. Operational Model of the Factory
OSTA must reflect fabrication logic, not repetitive discrete manufacturing. The operating pattern is
project-based, drawing-driven, BOM-driven, route-driven, and assembly-gated.

Layer Objects Meaning in OSTA

Explodes engineering demand
Project → FG → Assembly →
Demand hierarchy into controllable
Part
manufacturing objects

Material spec, stock, Links part demand to material
Material layer
procurement, allocation requirements and shortages

Represents real operational
Cutting, rolling, drilling, fit-up,
Execution hierarchy progress by work center and
welding, finishing
route step

Readiness, WIP, delay, Calculates what can move next
Control layer
completion and what is blocked

## 5. System Modules
Module Purpose Key Responsibilities

Project, FG, Assembly, Part,
Master Data Business structure definition Material, Routing, Work
Center, route standards

Validate part numbering,
revision, parent-child
BOM Import Layer Normalize engineering data
relationships, make/buy
classification

Demand explosion, MPS or
Decide what is needed and
Planning Layer MRP logic, shortage detection,
when
priority and due-date logic

Import nests, consumed
Nesting/Production Import Bridge planning with reality sheets, cut quantities, yield,
remnant, production events

Release, waiting material,
Execution/WIP Tracking Track actual status nested, cut complete, issued,
fit-up, welding, completed

Assembly readiness, delay
reason, blocked status,
Readiness and Control System-calculated decisions
completion and control
dashboards

## 6. Recommended Technology Stack
Recommended implementation for v1: PostgreSQL database, ASP.NET Core backend, EF Core ORM,
React + TypeScript frontend, modular monolith architecture, Dockerized local environment, and
Python helper scripts only where engineering-file parsing is needed.

Layer Recommendation Why

Strong relational modeling,
Database PostgreSQL constraints, indexing,
transactions, and reporting fit

Strong typing, maintainability,
Backend ASP.NET Core (.NET) transaction safety, clean
modular architecture

Good fit for relational domain
modeling, migrations, and
ORM EF Core
repository-free application
services

Fast internal-tool
development, strong
Frontend React + TypeScript
ecosystem for grids, filters,
and dashboards

Needed for BOM imports, nest
Background worker /
Jobs ingestion, recalculation,
scheduler
notifications, and snapshots

Useful for PDF, DXF, nesting,
and engineering-file
Support tooling Python utilities where needed
preprocessing without
polluting the core backend

## 7. Target Architecture
Start as a modular monolith. OSTA must centralize business rules while remaining easy to evolve.
Splitting into microservices too early would fragment identity control and transaction logic.
- Recommended backend solution structure:
- OSTA.Domain
- OSTA.Application
- OSTA.Infrastructure
- OSTA.API
- OSTA.Tests
- Architectural principles:
- Keep domain entities and invariants explicit.
- Prefer application services over generic repository abstraction when orchestrating workflows.
- Treat imports and production updates as transactional commands that append facts and update
state.
- Separate source transactions from calculated snapshots and reporting views.
- Design for auditability: who imported, when, from which file, what changed, and what failed
validation.

## 8. Core Domain Entities
Entity Type Purpose

Top-level demand container
Project Master for one fabrication job or
customer scope

FG / Equipment Master Finished good or fabricated

equipment under a project

Sub-structure grouping parts
Assembly Master
for execution and readiness

Core controlled object used
Part Master across engineering, nesting,
production, and assembly

Version-sensitive identity and
Part Revision Master
engineering control

Material spec linked to one or
Material Master
more parts

Standard route definition for a
Routing Master
part or part class

Operational resource where a
Work Center Master
route step executes

Header for one engineering
BOM Import Batch Transaction
import event

Exploded engineering line
BOM Line Transaction
linked to project structure

Imported nesting results and
Nest Header / Line Transaction
part cut linkage

Actual update that moves a
Production Event Transaction
part or assembly state

Reserved or issued material
Material Allocation Transaction
linked to demand

Latest computed state of each
Part Status Snapshot Computed
part

Latest readiness result and
Assembly Readiness Snapshot Computed blocking cause for each
assembly

## 9. Relationship Model
The backend data model must preserve the demand hierarchy and execution relationships. At
minimum, the following cardinalities must hold.

Relationship Rule

Project → FG One Project has many FGs

FG → Assembly One FG has many Assemblies

Assembly → Part One Assembly has many Parts

One Part typically requires one primary
Part → Material material spec, but extension must allow
alternates

One Part follows one routing definition at a
Part → Routing
given revision

One Nest contains many parts and one part
Nest → Part
may appear across multiple nest lines

One Assembly readiness result depends on
Assembly → Readiness
many part statuses and rules

One import batch contains many validated or
Import Batch → BOM Lines
rejected BOM lines

## 10. State Model
OSTA is not document-based; it is state-based. The state of each part and assembly must be derived
from transactions and guarded by rules.

Object Example States Notes

Draft, Released, Waiting
Material, Nested, Cut Actual state progression may
Part Complete, Issued to Assembly, vary by route and must be
In Fit-Up, In Welding, validation-aware
Completed

Not Ready, Ready for Fit-Up, In
Readiness must be calculated,
Assembly Fit-Up, In Welding, Completed,
with explicit blocking reason
Blocked

Import quality must be
Pending, Validated, Partially
Import Batch auditable and reversible
Accepted, Accepted, Failed
where needed

## 11. v1 API Module Breakdown
API Module Main Endpoints or Commands

List projects, get project detail, create project,
Projects
status summary

Hierarchy view, create or edit structure, get
FGs and Assemblies
readiness rollup

Part master search, part detail, revision
Parts
history, current status, route and material view

Upload batch, validate lines, accept import,
BOM Imports
view rejects, reprocess

Upload nest files, match parts, confirm cut
Nesting Imports
quantities, yield and remnant capture

Post event, update work-center progress,
Production Events
cancel or reverse under controlled rules

Recalculate assembly readiness, list blocked
Readiness
assemblies, explain blockers

Demand explosion, shortages, due dates,
Planning
material demand versus stock

Materials, routings, work centers, status
Reference Data
definitions, mappings

## 12. Database Schema Draft for v1
- Suggested first-wave tables:
- projects
- fgs
- assemblies
- parts
- part_revisions

- materials
- routings
- routing_steps
- work_centers
- bom_import_batches
- bom_lines
- nest_headers
- nest_lines
- production_events
- material_allocations
- part_status_snapshots
- assembly_readiness_snapshots
- audit_log
Minimum identity rule for uniqueness should include project context plus engineering identity. A
safe starting point is a constrained unique key built from project code, FG code, assembly code, part
number, and revision.

## 13. Clean-Code and Repository Rules
- Do not create business logic in controllers; controllers only translate HTTP to application
commands and queries.
- Do not hide core manufacturing logic inside frontend code.
- Do not name domain entities generically as Item, Data, Record, or Object when the business
meaning is known.
- Do not bypass validation rules during imports; rejected lines must be visible and traceable.
- Do not encode readiness as a manually editable flag when it should be calculated.
- Prefer explicit value objects and enums for statuses, codes, and route-step meaning.
- Every transaction that changes state should be auditable and, where appropriate, reversible by
controlled workflow.
- Write tests around domain rules first: identity matching, status progression, readiness blocking,
and import validation.

## 14. Copilot Grounding Rules
Paste or attach this document when working with Copilot and keep the following instructions
persistent in prompts:
- This system is a fabrication manufacturing control platform, not a generic CRUD app.
- The core identity is Project → FG → Assembly → Part → Revision.
- Preserve relational integrity and validation-heavy workflows.
- Prefer command and query workflows that reflect business transactions.
- Do not invent simplified shortcuts that break readiness or traceability logic.
- When generating code, explain assumptions and identify which values are entered, imported,
or computed.

- Keep modules small and cohesive; optimize for clean architecture and testability rather than
speed of scaffolding alone.
Recommended Copilot prompt starter:
Build this feature for OSTA according to the attached PRD and technical specification. Respect the fabrication
domain rules, keep the code clean and strongly typed, and separate master data, transactional data, and
computed state. Explicitly identify entities, validation rules, commands, queries, and tests.

## 15. First Implementation Sequence
Phase Goal Deliverables

Sample project, FG, assembly,
Understand and lock the
Phase 1 BOM, nest report, and status
domain model
examples; entity map; glossary

Projects, parts, assemblies,
Phase 2 Build the core schema materials, routings, import
batches, and audit tables

BOM upload and validation,
Phase 3 Implement transaction flows nest import, production event
posting, readiness calculation

Shortages, ready assemblies,
cut completion, delayed
Phase 4 Build control views
projects, material demand vs
stock

Permissions, audit trails, data
Phase 5 Harden the platform repair tools, performance
tuning, integration readiness

## 16. First Sprint Backlog
- Create solution structure and baseline architecture.
- Add PostgreSQL connection, EF Core migrations, and initial schema.
- Seed one sample project hierarchy.
- Implement Project, FG, Assembly, and Part masters.
- Implement BOM import batch and BOM line validation workflow.
- Build one screen that displays Project → FG → Assembly → Part tree.
- Add test coverage for identity consistency and duplicate prevention.

## 17. Anti-Patterns to Avoid
- Starting from dashboards before the domain model is stable.
- Treating engineering files as the final system of record instead of normalized OSTA data.
- Using free-text statuses instead of controlled state definitions.
- Allowing a part to be referenced differently in BOM, nest, and production modules.
- Building frontend pages first and forcing backend structure to match screen convenience.
- Turning computed readiness into a manual checkbox.
- Introducing microservices before the core domain and transactions are proven.

## 18. Definition of Success for v1
- A developer can trace one part from BOM import through nesting, production update, and
assembly readiness.
- The system can show which assemblies are ready for fit-up and explain why blocked assemblies
are blocked.
- The platform can distinguish entered values, imported values, and calculated values.
- Part identity remains consistent and enforceable across the full flow.
- The first production-control screens are useful without depending on spreadsheets as the
source of truth.
