#!/usr/bin/env python3
"""
Remote Database Deployment Script for RailCat Platform (Private Restaurant / Curry Point)
Provisions the 4 clean, minimal 'railcat_*' databases and roles on target PostgreSQL server.
"""

import os
import sys
import psycopg2
from psycopg2 import sql, extensions
from pathlib import Path

DB_CONFIG = {
    'host': os.getenv('DB_HOST', '103.149.199.247'),
    'port': int(os.getenv('DB_PORT', 5432)),
    'user': os.getenv('DB_USER', 'postgres'),
    'password': os.getenv('DB_PASSWORD', 'missileman123'),
}

SERVICES = [
    {
        'db_name': 'railcat_users',
        'role_name': 'railcat_users_user',
        'role_pass': 'railcat_users_secret_2026',
        'scripts': [
            'schemas/01_railcat_users/01_schema.sql',
            'schemas/01_railcat_users/02_seed.sql'
        ]
    },
    {
        'db_name': 'railcat_catalog',
        'role_name': 'railcat_catalog_user',
        'role_pass': 'railcat_catalog_secret_2026',
        'scripts': [
            'schemas/02_railcat_catalog/01_schema.sql',
            'schemas/02_railcat_catalog/02_seed.sql'
        ]
    },
    {
        'db_name': 'railcat_orders',
        'role_name': 'railcat_orders_user',
        'role_pass': 'railcat_orders_secret_2026',
        'scripts': [
            'schemas/03_railcat_orders/01_schema.sql',
            'schemas/03_railcat_orders/02_queue_function.sql',
            'schemas/03_railcat_orders/03_seed.sql'
        ]
    },
    {
        'db_name': 'railcat_settlement',
        'role_name': 'railcat_settlement_user',
        'role_pass': 'railcat_settlement_secret_2026',
        'scripts': [
            'schemas/04_railcat_settlement/01_schema.sql',
            'schemas/04_railcat_settlement/02_seed.sql'
        ]
    }
]

BASE_DIR = Path(__file__).resolve().parent

def deploy():
    print("=" * 70)
    print("RailCat Private Restaurant Platform - Safe Database Deployment")
    print(f"Target Host: {DB_CONFIG['host']}:{DB_CONFIG['port']}")
    print("Naming Convention: 'railcat_' prefix for all databases and roles")
    print("=" * 70)

    # 1. Connect to postgres maintenance DB to check roles and create databases
    print("\n[Step 1/3] Connecting to master server to check existing databases and roles...")
    master_conn = psycopg2.connect(**DB_CONFIG, dbname='postgres')
    master_conn.set_isolation_level(extensions.ISOLATION_LEVEL_AUTOCOMMIT)
    master_cur = master_conn.cursor()

    master_cur.execute("SELECT datname FROM pg_database WHERE datistemplate = false;")
    existing_dbs = {row[0] for row in master_cur.fetchall()}
    print(f"Server currently hosts {len(existing_dbs)} databases.")

    # Create service roles safely
    for svc in SERVICES:
        role = svc['role_name']
        pwd = svc['role_pass']
        master_cur.execute("SELECT 1 FROM pg_roles WHERE rolname = %s;", (role,))
        if not master_cur.fetchone():
            print(f"  + Creating role: {role}")
            master_cur.execute(sql.SQL("CREATE ROLE {} WITH LOGIN PASSWORD %s;").format(sql.Identifier(role)), [pwd])
        else:
            print(f"  . Role already exists: {role}")

    # Create service databases safely
    for svc in SERVICES:
        db = svc['db_name']
        role = svc['role_name']
        if db not in existing_dbs:
            print(f"  + Creating isolated database: {db} (Owner: {role})")
            master_cur.execute(sql.SQL("CREATE DATABASE {} OWNER {};").format(sql.Identifier(db), sql.Identifier(role)))
        else:
            print(f"  . Database already exists (preserving): {db}")

    master_cur.close()
    master_conn.close()

    # 2. Connect to each service database individually and apply schemas/seeds
    print("\n[Step 2/3] Executing DDL schemas, queue functions, and seed data per database...")
    for svc in SERVICES:
        db = svc['db_name']
        print(f"\n---> Configuring Database: [{db}]")
        conn = psycopg2.connect(**DB_CONFIG, dbname=db)
        conn.autocommit = True
        cur = conn.cursor()

        # Enable pgcrypto extension
        cur.execute("CREATE EXTENSION IF NOT EXISTS pgcrypto;")
        print("     Installed extension: pgcrypto")

        # Execute scripts in sequence
        for rel_path in svc['scripts']:
            file_path = BASE_DIR / rel_path
            if not file_path.exists():
                raise FileNotFoundError(f"Missing script file: {file_path}")
            
            print(f"     Executing: {rel_path} ...", end=" ")
            with open(file_path, "r", encoding="utf-8") as f:
                sql_content = f.read()
            
            cur.execute(sql_content)
            print("OK")

        cur.close()
        conn.close()

    # 3. Validation and Verification
    print("\n[Step 3/3] Validating tables and row counts across all 4 railcat databases...")
    print("-" * 70)
    for svc in SERVICES:
        db = svc['db_name']
        conn = psycopg2.connect(**DB_CONFIG, dbname=db)
        cur = conn.cursor()
        cur.execute("""
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            ORDER BY table_name;
        """)
        tables = [row[0] for row in cur.fetchall()]
        
        table_stats = []
        for tbl in tables:
            cur.execute(sql.SQL("SELECT COUNT(*) FROM {};").format(sql.Identifier(tbl)))
            count = cur.fetchone()[0]
            table_stats.append(f"{tbl} ({count} rows)")
        
        print(f"[{db}]: {len(tables)} tables")
        for stat in table_stats:
            print(f"    - {stat}")
        
        cur.close()
        conn.close()

    print("\n" + "=" * 70)
    print("SUCCESS: All 4 'railcat_*' databases provisioned and seeded successfully!")
    print("Zero other databases or schemas were touched or modified.")
    print("=" * 70)

if __name__ == "__main__":
    deploy()
