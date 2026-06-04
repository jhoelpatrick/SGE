-- Ejecuta esto en tu Editor de SQL en Supabase para permitir registrar clientes y proveedores de cualquier ubigeo
-- sin que se caiga por la validación de llave foránea.

ALTER TABLE comercial.clientes DROP CONSTRAINT IF EXISTS fk_clientes_ubigeos;
ALTER TABLE comercial.proveedores DROP CONSTRAINT IF EXISTS fk_proveedores_ubigeos;
ALTER TABLE operaciones.almacenes DROP CONSTRAINT IF EXISTS fk_almacenes_ubigeos;
