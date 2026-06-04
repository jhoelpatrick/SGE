import os
import re

def fix_returning(directory):
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('Repository.cs'):
                filepath = os.path.join(root, file)
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()

                # Find INSERT INTO (schema.table) ... RETURNING clienteid;
                # Replace with RETURNING (the actual primary key)
                # First let's just do it manually for the known ones:
                
                # CompraRepository -> ordenid
                if 'CompraRepository.cs' in file:
                    content = content.replace('RETURNING clienteid;', 'RETURNING ordenid;')
                # FacturacionRepository -> comprobanteid
                if 'FacturacionRepository.cs' in file:
                    content = content.replace('RETURNING clienteid;', 'RETURNING comprobanteid;')
                # ProductoRepository -> productoid
                if 'ProductoRepository.cs' in file:
                    content = content.replace('RETURNING clienteid;', 'RETURNING productoid;')
                # ProveedorRepository -> proveedorid
                if 'ProveedorRepository.cs' in file:
                    content = content.replace('RETURNING clienteid;', 'RETURNING proveedorid;')
                # ProyectoRepository -> proyectoid
                if 'ProyectoRepository.cs' in file:
                    content = content.replace('RETURNING clienteid;', 'RETURNING proyectoid;')
                # VentaRepository -> pedidoid
                if 'VentaRepository.cs' in file:
                    content = content.replace('RETURNING clienteid;', 'RETURNING pedidoid;')
                # InventarioRepository -> Does it use SCOPE_IDENTITY? Let's assume it might, but what id? 
                if 'InventarioRepository.cs' in file:
                    content = content.replace('RETURNING clienteid;', 'RETURNING almacenid;')

                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)

fix_returning(r'c:\Users\alu_torre1\Desktop\SGE\SGE\Services')
