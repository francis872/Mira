# Investigación y decisiones V2

El modelo no estaba en la rama de trabajo: se localizó en `repair/v1-catalogos`, commit `880ac75`. El usuario confirmó expresamente los dos archivos SQL como oficiales. Se preservaron sus bytes y se comparó el modelo incorporado contra la fuente real; no se diseñó un sustituto provisional.

El SQL plural difiere de V1 singular, por lo que se incorpora de forma aditiva. Las referencias oficiales son sede→grupo y grupo→semillero; no se introduce universidad→grupo. Personas de docentes son un catálogo de solo lectura, sin superficie de credenciales ni autenticación. Si no hay personas, el selector informa el vacío.

Los recursos comparten validación y despacho por una lista cerrada de metadatos. Repository verifica que la definición sea una instancia registrada y parametriza todos los valores. Las 78 funciones explícitas están en SQL; `scripts/generate-research-routines.py` permite regenerarlas. La ficha usa un solo comando de base y bloqueo de maestro; una excepción posterior revierte todas las escrituras.

El SQL carece de FK opcionales. C06 no se declara satisfecho con una columna inventada ni con un campo de texto opcional. Siguen pendientes los documentos originales del curso que determinen su no aplicabilidad y los nombres de nueve documentos. Docker y producción siguen sin verificar; PostgreSQL 16.15 sí se ejecutó.
