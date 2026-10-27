# Reparación de colisiones de Scene1

Fecha: 27 de octubre de 2026. Rama de trabajo: `Juandate`.

## Escena y diagnóstico

La revisión se ejecutó dentro de Unity 6000.5.6f1 sobre la escena abierta `Assets/Game/Scenes/Scene1.unity`, mediante una herramienta de editor. Se inspeccionaron 2.916 mallas y 2.887 colliders iniciales.

El rig ya disponía de CharacterController, movimiento continuo de XRI con gravedad y XRBodyTransformer configurado para usar la cápsula. La matriz de capas permitía los contactos. No había MeshColliders sin malla ni combinaciones inválidas de MeshCollider cóncavo y Rigidbody dinámico.

Los principales defectos encontrados fueron:

- 24 piezas del mapa sin collider sólido: ocho paños de suelo, cinco de techo, seis paredes, dos vigas, dos hojas de puerta y un marco de puerta.
- 34 paños de suelo con superficies de colisión muy finas, sustituidos por losas con volumen para mejorar el apoyo del jugador.
- Dos piezas `SM_DG_HG_Wall02D` cuyo collider importado respondía a rayos pero permitía cruzar la cápsula en las pruebas. Se reforzó el cuerpo de mampostería con una caja ajustada, conservando el collider de los detalles superiores.
- La cápsula inicial tenía un centro vertical desalineado y el XR Origin estaba en Y=15,33, muy por encima del suelo situado aproximadamente en Y=0,87.

## Cambios aplicados

### Geometría

- Añadidos 23 BoxColliders para las piezas simples que no tenían colisión y un MeshCollider estático preciso para el marco de puerta, conservando su abertura.
- Sustituidos 34 MeshColliders finos de suelo por BoxColliders de 12 cm de grosor, manteniendo la cara superior en la superficie existente. Los colliders anteriores permanecen desactivados para evitar doble contacto.
- Añadidas dos cajas de refuerzo en las paredes que fallaban los barridos. Las cajas se ajustan al cuerpo de la pared y excluyen el vuelo de los aleros.
- Los cambios son propios de la escena. No se modificaron las mallas ni los prefabs originales del paquete.
- Resultado: 2.913 colliders sólidos activos y ninguna malla del mapa sin collider sólido asociado. Los 34 colliders reemplazados quedan desactivados.

### Jugador

- CharacterController habilitado, con detección de colisiones activa.
- Radio de 0,25 m, skin width de 0,025 m, step offset de 0,20 m y min move distance de cero.
- Altura y centro inicial alineados con la altura de la cámara. El manipulador corporal estándar de XRI continúa ajustándolos durante la locomoción.
- Pies del jugador colocados aproximadamente en `(-28.37, 0.89, 0.00)`, conservando la posición horizontal y apoyándose en el suelo del interior.
- Añadido `XRHeadCollisionGuard`: oscurece la vista si el visor entra en una pared o cruza hacia su otro lado mediante movimiento físico. La vista se recupera al volver al lado seguro. No modifica la pose de seguimiento de la cámara.

En VR, una colisión del cuerpo no puede detener físicamente la cabeza ni las manos del usuario. El CharacterController bloquea la locomoción con el mando; el oscurecimiento evita mirar a través de la arquitectura al inclinarse o caminar físicamente. Las manos controladas directamente por tracking requieren su propio diseño de interacción física si se quiere restringirlas también.

## Validación realizada

Las pruebas se ejecutaron en Unity Editor con copias de los colliders y una cápsula de jugador en una escena temporal de previsualización, sin alterar el recorrido guardado.

| Comprobación | Resultado |
|---|---|
| Caídas sobre los centros de 108 colliders de suelo | 108 superadas |
| Barridos de cápsula contra paredes y puertas desde ambos lados | 424 superados |
| Cabeza simulada fuera de geometría | Vista normal |
| Cabeza simulada dentro de una pared | Vista oscurecida |
| Cabeza que cruza completamente una pared | Mantiene el oscurecimiento |
| Retorno de la cabeza al lado seguro | Libera el oscurecimiento |
| Mallas del mapa sin collider sólido asociado | 0 |
| Compilación de scripts en Unity | Correcta |

Estas comprobaciones cubren puntos y recorridos representativos; no constituyen un recorrido exhaustivo de cada borde del palacio ni una prueba de comodidad con visor físico. Queda realizar una pasada en el dispositivo objetivo por juntas de suelo, esquinas, puertas, escalones y los límites del área jugable.

## Uso y archivos

- Escena reparada: `Assets/Game/Scenes/Scene1.unity`.
- Protección del visor: `Assets/Game/Scripts/XRHeadCollisionGuard.cs`.
- Herramienta de editor: `Assets/Game/Editor/CollisionSceneAudit.cs`.
- Menús disponibles: `Tools > Daejojeon > Audit Scene Collisions`, `Repair Scene Collisions` y `Validate Scene Collisions`.
- Informes detallados de esta sesión: `Library/CollisionReports/`. Library no se sube a Git.
- Copia de respaldo anterior a la reparación: `Library/CollisionReports/Scene1-before-collision-repair.unity`.

La reparación automática está limitada a Scene1 y requiere detener Play y guardar cualquier edición pendiente. Debe revisarse si se modifica de forma importante el diseño del mapa.

La configuración de compilación consultada habilita `SampleScene`. Para probar este escenario en una compilación del visor, incluir y seleccionar `Scene1` en el perfil de compilación correspondiente.

## Criterio sobre Convex

No se activó Convex de forma masiva. Un MeshCollider estático puede ser cóncavo, y convertir un conjunto con puertas o huecos a una sola envolvente convexa puede cerrarlos. Para las superficies planas se prefirieron cajas con volumen, que tienen menor complejidad que una malla de render y no dependen del sentido de sus triángulos.

Referencia: [documentación oficial de Unity sobre MeshColliders](https://docs.unity.com/en-us/engine/6000.3/manual/physics-section/physics-overview/collision-section/collider-shapes/mesh-colliders/introduction).

## Actualización posterior: Convex en todos los MeshColliders

Por solicitud explícita del usuario, se activó Convex en los 2.882 MeshColliders presentes en Scene1, incluidos los desactivados. Se cambiaron 2.874 componentes; ocho ya tenían Convex activo. Se guardó la escena conservando los cambios que estaban abiertos en el editor. Unity no emitió mensajes de fallo de generación convexa durante la operación.

Este ajuste sustituye la decisión anterior de mantener algunas mallas cóncavas. Las pruebas físicas documentadas arriba corresponden a la configuración anterior; los huecos y pasos deben revisarse con la nueva envolvente convexa. La copia de respaldo anterior a este cambio está en `Library/CollisionReports/Scene1-before-convex.unity`.
