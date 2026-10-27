# Puertas deslizantes Door08 para VR

## Resultado

Las 65 hojas `SM_DG_Door08` que estaban en Scene1 se agruparon por modelo, posición y orientación. Se añadió una hoja gemela a `SM_DG_Door08E (2)`, por indicación del usuario, para completar la puerta que estaba incompleta.

La escena contiene **33 puertas de dos hojas, con 66 hojas en total**. Cada puerta es una instancia de prefab. Se retiraron los componentes de interacción y movimiento de `SM_DG_Door04G (1)` y `SM_DG_Door04H (1)` y se eliminó el controlador anterior `VR - Puerta deslizante`.

## Prefabs

Todos están en `Assets/Game/prefabs`:

| Prefab | Uso |
|---|---|
| `SM_DG_Door08A_Doble.prefab` | Modelo A: 12 puertas en la escena. |
| `SM_DG_Door08B_Doble.prefab` | Modelo B: 8 puertas. |
| `SM_DG_Door08C_Doble.prefab` | Modelo C: 6 puertas; la pareja seleccionada C5/C6 sirvió como referencia. |
| `SM_DG_Door08D_Doble.prefab` | Modelo D: 2 puertas. |
| `SM_DG_Door08E_Doble.prefab` | Modelo E de tamaño normal: 3 puertas. |
| `SM_DG_Door08E_Doble_Ancha.prefab` | Modelo E de tamaño ajustado: 2 puertas, incluida la pareja completada con la gemela. |

El prefab contiene un `SlidingDoorGroup` y dos hojas con `SlidingDoorHandle`, Rigidbody cinemático y collider sólido. La escena conserva los nombres originales de las hojas dentro de las nuevas instancias, así como las posiciones, rotaciones, escalas y materiales existentes. Los detalles visuales particulares se mantienen mediante ajustes de instancia.

## Uso

Acercar una mano a cualquiera de las hojas, agarrarla y desplazar la mano en el sentido de apertura. Un tirón de **7 cm** activa el recorrido completo de ambas hojas; se puede soltar la mano mientras la puerta termina de abrirse. El movimiento tarda aproximadamente **0,9 s**. Para cerrar, agarrar una hoja abierta y tirar en el sentido inverso.

Si el cuerpo del jugador ocupa el recorrido, la puerta se detiene y continúa al despejarse el paso. El comportamiento utiliza los scripts existentes `SlidingDoorGroup` y `SlidingDoorHandle`, cuyo código se conservó.

Para colocar otra puerta, arrastrar el prefab del modelo adecuado a la escena. En el componente `Sliding Door Group`, `Panels` identifica las dos hojas, `Open Offsets` define el recorrido local de cada una, `Pull Distance` controla el tirón necesario y `Duration` controla la duración. Los marcos visuales existentes se conservan aparte de los prefabs de las hojas.

## Colisiones y validación

Se corrigieron cuatro marcos relacionados con Door08 cuya envolvente convexa tapaba el hueco, sustituyendo su contacto por laterales y dintel. Los colliders de las hojas se desplazan con sus Rigidbody cinemáticos.

Se verificaron en Unity las 33 instancias: umbral del tirón, apertura completa, espacio para una cápsula de jugador, pausa al encontrar al jugador y cierre tras liberar el recorrido. Las 33 pruebas de paso entre hojas fueron correctas y no se detectaron colliders de marco bloqueando los pasos. También se comprobaron los Rigidbody, los componentes de interacción y las referencias internas de los seis prefabs.

Las simulaciones se realizaron en una escena física de previsualización aislada. Queda probar el alcance y la comodidad del agarre con el visor real.

Los informes detallados y la copia de seguridad anterior a la migración se guardan en `Library/Door08Migration`, que no se versiona. La instantánea preparada para el commit contiene la migración y los cambios relacionados con Door08; los demás cambios pendientes del usuario se conservaron aparte.

La comparación visual con la copia original comprobó las 66 mallas originales: posición mundial, geometría, materiales y visibilidad coinciden, sin detalles visibles adicionales. La gemela autorizada se contabiliza como la nueva hoja 66, además de las 65 hojas originales.
