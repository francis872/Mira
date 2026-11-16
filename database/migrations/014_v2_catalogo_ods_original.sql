-- HU09-CA02 exige explícitamente los 17 ODS. No altera el catálogo singular V1.
-- Nombres: https://www.undp.org/es/sustainable-development-goals
-- Conserva IDs, nombres y relaciones existentes, incluidos los dos rótulos de 006.
INSERT INTO public.ods(nombre)
SELECT 'ODS ' || v.numero || ' - ' || v.nombre
FROM (VALUES
 (1,'Fin de la pobreza'),(2,'Hambre cero'),(3,'Salud y bienestar'),
 (4,'Educación de calidad'),(5,'Igualdad de género'),(6,'Agua limpia y saneamiento'),
 (7,'Energía asequible y no contaminante'),(8,'Trabajo decente y crecimiento económico'),
 (9,'Industria, innovación e infraestructura'),(10,'Reducción de las desigualdades'),
 (11,'Ciudades y comunidades sostenibles'),(12,'Producción y consumo responsables'),
 (13,'Acción por el clima'),(14,'Vida submarina'),(15,'Vida de ecosistemas terrestres'),
 (16,'Paz, justicia e instituciones sólidas'),(17,'Alianzas para lograr los objetivos')
) AS v(numero,nombre)
WHERE NOT EXISTS (
 SELECT 1 FROM public.ods o
 WHERE o.nombre='ODS ' || v.numero || ' - ' || v.nombre
 OR (v.numero=4 AND o.nombre='ODS 4 - Educacion de Calidad')
 OR (v.numero=9 AND o.nombre='ODS 9 - Industria, Innovacion e Infraestructura')
)
ON CONFLICT (nombre) DO NOTHING;
