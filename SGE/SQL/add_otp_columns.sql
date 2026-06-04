-- Agregar columnas para autenticación OTP a la tabla de usuarios
ALTER TABLE rrhh_recursos.usuarios_nomina
ADD COLUMN IF NOT EXISTS otpcode VARCHAR(6),
ADD COLUMN IF NOT EXISTS otpexpiry TIMESTAMP;
