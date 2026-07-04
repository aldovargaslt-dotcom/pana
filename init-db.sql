-- Initial seed data for development
-- Creates the default bakery tenant

INSERT INTO tenants (id, name, slug, is_active, created_at)
VALUES ('00000000-0000-0000-0000-000000000001', 'Default Bakery', 'default-bakery', true, NOW())
ON CONFLICT (slug) DO NOTHING;

-- ── Default Units of Measure ──────────────────────────────────
-- Weight category (reference: kg = factor 1)
INSERT INTO "UnitOfMeasures" ("Id", "TenantId", "Name", "Symbol", "Category", "Factor", "RoundingPrecision", "IsActive", "CreatedAt")
VALUES
    ('00000000-0000-0000-0000-000000000101', '00000000-0000-0000-0000-000000000001', 'Kilogram', 'kg', 'Weight', 1, 0.001, true, NOW()),
    ('00000000-0000-0000-0000-000000000102', '00000000-0000-0000-0000-000000000001', 'Gram', 'g', 'Weight', 0.001, 1, true, NOW()),
    ('00000000-0000-0000-0000-000000000103', '00000000-0000-0000-0000-000000000001', 'Pound', 'lb', 'Weight', 0.453592, 0.01, true, NOW())
ON CONFLICT DO NOTHING;

-- Unit category (reference: unit = factor 1)
INSERT INTO "UnitOfMeasures" ("Id", "TenantId", "Name", "Symbol", "Category", "Factor", "RoundingPrecision", "IsActive", "CreatedAt")
VALUES
    ('00000000-0000-0000-0000-000000000201', '00000000-0000-0000-0000-000000000001', 'Unit', 'u', 'Unit', 1, 1, true, NOW()),
    ('00000000-0000-0000-0000-000000000202', '00000000-0000-0000-0000-000000000001', 'Dozen', 'dz', 'Unit', 12, 1, true, NOW()),
    ('00000000-0000-0000-0000-000000000203', '00000000-0000-0000-0000-000000000001', 'Box', 'box', 'Unit', 24, 1, true, NOW())
ON CONFLICT DO NOTHING;

-- Volume category (reference: liter = factor 1)
INSERT INTO "UnitOfMeasures" ("Id", "TenantId", "Name", "Symbol", "Category", "Factor", "RoundingPrecision", "IsActive", "CreatedAt")
VALUES
    ('00000000-0000-0000-0000-000000000301', '00000000-0000-0000-0000-000000000001', 'Liter', 'L', 'Volume', 1, 0.01, true, NOW()),
    ('00000000-0000-0000-0000-000000000302', '00000000-0000-0000-0000-000000000001', 'Milliliter', 'mL', 'Volume', 0.001, 1, true, NOW())
ON CONFLICT DO NOTHING;

-- ── Default Stock Locations ───────────────────────────────────
INSERT INTO "StockLocations" ("Id", "TenantId", "Name", "Code", "LocationType", "ParentLocationId", "IsActive", "CreatedAt")
VALUES
    ('00000000-0000-0000-0000-000000000401', '00000000-0000-0000-0000-000000000001', 'Main Warehouse', 'WH-MAIN', 'Warehouse', NULL, true, NOW()),
    ('00000000-0000-0000-0000-000000000402', '00000000-0000-0000-0000-000000000001', 'Storage Zone', 'ZONE-STOR', 'Zone', '00000000-0000-0000-0000-000000000401', true, NOW()),
    ('00000000-0000-0000-0000-000000000403', '00000000-0000-0000-0000-000000000001', 'Production Zone', 'ZONE-PROD', 'Production', '00000000-0000-0000-0000-000000000401', true, NOW()),
    ('00000000-0000-0000-0000-000000000404', '00000000-0000-0000-0000-000000000001', 'Dry Ingredients Shelf', 'SH-DRY', 'Shelf', '00000000-0000-0000-0000-000000000402', true, NOW()),
    ('00000000-0000-0000-0000-000000000405', '00000000-0000-0000-0000-000000000001', 'Cold Storage Shelf', 'SH-COLD', 'Shelf', '00000000-0000-0000-0000-000000000402', true, NOW()),
    ('00000000-0000-0000-0000-000000000406', '00000000-0000-0000-0000-000000000001', 'Front Display', 'ZONE-FRONT', 'Zone', '00000000-0000-0000-0000-000000000401', true, NOW())
ON CONFLICT DO NOTHING;
