# El último sueño de Daejojeon — Especificaciones técnicas y mecánicas VR

**Estado:** propuesta de diseño para un primer producto jugable; no representa funcionalidades ya implementadas.  
**Referencia narrativa:** `historia-base.md`.  
**Experiencia objetivo:** terror psicológico para una persona, de 10 a 15 minutos, en tres espacios conectados.  
**Prioridad:** terminar una experiencia completa, comprensible y cómoda con el menor número posible de sistemas y recursos nuevos.

## 1. Idea central

El jugador usa sus manos para explorar y resolver un único acertijo mientras vigila a la Reina del Silencio. Cuando la observa, ella permanece inmóvil; cuando aparta la vista durante el tiempo suficiente, desaparece y puede reaparecer más cerca. La tensión nace de tener que mirar las cartas, las manos y los objetos sin perder de vista a la amenaza.

No hay combate, armas, inventario complejo, enemigos adicionales ni persecución con navegación libre en la versión inicial. La Reina se desplaza entre posiciones preparadas de antemano. El cambio de los pasillos se representa con puertas, iluminación y sonido, sin generación procedural.

## 2. Base técnica existente y supuestos

- El repositorio declara Unity **6000.5.6f1**, Universal Render Pipeline 17.5.0, Input System 1.20.0, XR Interaction Toolkit 3.5.1, XR Management 4.5.4 y OpenXR 1.17.1.
- Incluye los Starter Assets y el XR Device Simulator del XR Interaction Toolkit; también existen scripts `XRLever` y `XRPushButton` en `Assets/UI_3D/UI_3D/Scripts`, cuya integración y compatibilidad se deben comprobar en una escena de prueba.
- Hay un `TagSocket` y un `InteractionManager` en `Assets/Game/Scripts`, pero están ligados a paneles y etiquetas de otro prototipo. Pueden estudiarse como referencia; el acertijo necesita su propia validación simple, sin asumir que esos scripts ya lo resuelven.
- Se propone usar dos mandos VR con agarre, gatillo y palancas analógicas. El dispositivo objetivo y su presupuesto de rendimiento aún están por definir. El seguimiento de manos y ojos no es necesario.
- **Estado local observado:** `Assets/DaejojeonComplex` aparece eliminado en la copia de trabajo y existen otros cambios sin confirmar. Antes de montar el nivel hay que comprobar con el equipo si el escenario fue movido o retirado; no restaurar ni sobrescribir cambios sin entenderlos.
- Valores como tiempos, distancias y velocidades son puntos de partida para pruebas, no resultados medidos.

## 3. Recorrido jugable

| Momento | Tiempo orientativo | Acción y resultado |
|---|---:|---|
| Entrada y aprendizaje | 1–2 min | Moverse, tomar el farol, abrir una puerta y encontrar la primera carta. No hay captura. |
| Corredor | 2–3 min | La Reina aparece a distancia. El jugador aprende visual y acústicamente que queda quieta al ser observada y avanza cuando se deja de mirar. |
| Habitaciones de la reina | 3–5 min | Leer una pista, encontrar tres sellos y colocarlos en un pequeño altar en el orden indicado. La Reina vuelve a aparecer en un encuentro delimitado. |
| Cámara del documento | 2–3 min | Abrir la cámara tras resolver el acertijo y recoger el documento. Su lectura aclara lo sucedido y habilita la salida. |
| Decisión y desenlace | 1–2 min | Acercar el documento al brasero para destruirlo o conservarlo y salir con él. Una captura conduce al desenlace de quedar atrapado. |

Los tiempos se ajustan con pruebas. El camino principal debe ser lineal, con una bifurcación final clara; no depende de memorizar un laberinto.

## 4. Controles y comodidad VR

| Acción | Entrada propuesta | Comportamiento |
|---|---|---|
| Movimiento | Stick izquierdo | Desplazamiento continuo relativo a la orientación horizontal de la cabeza; velocidad inicial ajustable de 1,2 m/s. |
| Giro | Stick derecho | Giro por pasos de 45°; zona muerta y repetición controlada para evitar giros múltiples. |
| Agarrar y soltar | Grip de cada mando | Selección directa de objetos o tiradores cercanos; soltar libera el objeto. Ambas manos tienen las mismas capacidades. |
| Interactuar a distancia | Rayo desde el mando y gatillo | Solo para UI y, si resulta necesario en pruebas, objetos fuera del alcance físico; mantener agarre directo como interacción principal. |
| Teletransporte | Acción configurable del mando | Alternativa opcional al desplazamiento continuo, usando superficies y puntos previamente habilitados. Nunca debe atravesar puertas bloqueadas ni permitir entrar en paredes. |
| Pausa y ajustes | Botón de menú | Pausar amenaza y tiempo de juego; permitir reiniciar desde punto de control y ajustar movimiento. |

Utilizar `XR Origin`, interacción directa con ambas manos, `XR Interaction Manager`, locomoción y acciones de los Starter Assets de XRI como punto de partida. Los mapeos exactos dependen del visor y perfil OpenXR finalmente elegido. No desplazar ni girar la cámara del visor por código para sustos, no bloquear la cabeza y no usar destellos intensos. Dejar jugar de pie o sentado y mantener pistas y puntos de interacción a una altura accesible. La cinemática de captura usa sonido y fundido breve, sin sacudir la cámara.

## 5. Mecánicas e interacciones del entorno

### 5.1. Mirada y vigilancia

- La «mirada» se aproxima mediante la posición y orientación de la cámara del visor; no requiere seguimiento ocular.
- La Reina se considera observada si un punto del torso o de la cabeza está dentro de un cono configurable de **70° de ángulo total**, está a una distancia máxima inicial de 12 m y tiene línea de visión despejada. Comprobar dos puntos para reducir falsos negativos por muebles; ignorar colliders propios del jugador y del personaje.
- Mientras está observada, permanece quieta. Tras **0,75 s continuos** fuera de vista, desaparece; después de **1,5–3 s**, reaparece en una posición válida más cercana. Si se la vuelve a mirar antes de terminar los 0,75 s, el temporizador se reinicia.
- El margen evita que un giro breve, parpadeo de detección o borde de pantalla provoque un avance injusto. Los valores se ajustan en visor; el frustum real puede no coincidir con el cono, por lo que se debe comprobar la sensación en ambos ojos.
- Las pistas son físicas y auditivas: silueta iluminada, susurro direccional y un sonido discreto cuando cambia de posición. No se requiere HUD permanente.

### 5.2. Farol y exploración

El jugador recoge un farol con cualquiera de las manos y lo orienta naturalmente. La luz ayuda a ver pistas y localizar a la Reina; **no la paraliza ni causa daño**. No consume batería y puede soltarse en superficies. Al reiniciar un punto de control, el farol reaparece en un soporte conocido si se perdió. Su luz es sencilla y su alcance se ajusta según rendimiento; la mecánica no depende de sombras dinámicas.

### 5.3. Cartas y documento

- Dos cartas breves y el documento final son objetos agarrables con `XR Grab Interactable`, collider y `Rigidbody` configurado para que no atraviesen el escenario.
- Acercar y girar una carta permite leer texto grande, con contraste suficiente. Los textos contienen la historia y una pista explícita para el acertijo; no son obligatorios como condición de activación, salvo recoger el documento final.
- El documento final tiene un estado único: `Disponible`, `En posesión` o `Destruido`. Para salir con él, debe estar agarrado en el instante de cruzar la salida; si se suelta, puede recuperarse. Para evitar pérdidas, un objeto caído fuera del área vuelve a su mesa o soporte.
- No se necesita inventario ni combinaciones de objetos en la mano.

### 5.4. Puertas

Usar dos puertas funcionales y una puerta de salida. El jugador toma el tirador cercano y mueve la mano: la puerta gira o se desliza dentro de límites predefinidos. Para rapidez, implementar movimiento restringido mediante el desplazamiento proyectado de la mano y un transform cinemático, sin simulación física compleja. El tipo de movimiento debe ajustarse al modelo del escenario.

- Puerta normal: comienza cerrada, puede abrirse y cerrarse; su collider evita atravesarla.
- Puerta de la cámara: bloqueada hasta resolver los tres sellos; al intentar abrirla suena un mecanismo y se nota una resistencia visual, sin forzar la mano virtual.
- Puerta final: se desbloquea al recoger el documento. Cruzar su umbral resuelve el final según el estado del documento y su posesión.
- Los cierres sobrenaturales se ejecutan únicamente en transiciones definidas y cuando el jugador no ocupa el umbral. No cerrar puertas sobre el jugador, el farol ni el documento.

### 5.5. Acertijo de los tres sellos

Hay tres piezas con símbolos distintos y tres soportes sobre un altar. Una carta muestra el orden mediante dibujos claros, reforzados con texto. El jugador recoge cada sello y lo coloca en el soporte que corresponda. Cada soporte acepta solo su sello usando una identidad o enum; al colocarlo se ajusta a una posición y emite sonido. Una pieza incorrecta no se acopla y permanece recuperable.

Al completar los tres soportes, el altar confirma el éxito con luz y sonido y desbloquea la cámara de manera permanente. La solución no puede perderse por soltar una pieza; una pieza fuera del área reaparece en su posición inicial. Emplear `XR Socket Interactor` y un controlador del acertijo que escuche eventos de inserción y extracción. Mantener el reto en la manipulación y la vigilancia, sin cronómetro de acertijo separado.

### 5.6. Botones y palancas

La versión mínima usa **una palanca física** junto al altar para abrir el panel que revela los soportes. Se agarra y desplaza hasta su posición final; reproduce sonido y queda activada. Si el componente `XRLever` existente funciona correctamente con la versión y los mandos elegidos, reutilizarlo; de lo contrario implementar una interacción restringida de dos estados.

Los botones físicos son **opcionales** y no bloquean el progreso del primer prototipo. Si se añade uno, usarlo para encender una lámpara ambiental o confirmar un ajuste, con respuesta visual, sonido y presión directa. Evitar introducir un segundo acertijo basado en botones. La UI de pausa puede usar un canvas en el mundo y selección por rayo.

## 6. Reina del Silencio: comportamiento técnico

### Máquina de estados

1. **Inactiva:** no existe amenaza; espera un evento narrativo o la entrada en una zona.
2. **Aparición:** se activa el modelo en un punto marcado, a distancia segura y preferiblemente visible; sonido de advertencia. Durante la primera aparición queda quieta hasta que el jugador la observe una vez.
3. **Observada:** permanece inmóvil y el temporizador de pérdida de vista se reinicia.
4. **Fuera de vista:** acumula tiempo sin línea de visión; al llegar al umbral desaparece y espera un intervalo.
5. **Reposicionamiento:** elige el siguiente punto válido de una lista ordenada, situado más cerca del jugador, en suelo válido y fuera del cono de visión. Se muestra y vuelve a evaluar la mirada.
6. **Captura:** si, en un encuentro activo, la Reina está a menos de **1,2 m** del jugador durante **1 s continuo**, suena un aviso y se inicia un fundido. Se pierde el encuentro y se muestra el desenlace de quedar atrapado, con opción de reiniciar el punto de control.
7. **Desactivada:** al completar el encuentro, llegar a una zona segura o resolver un final, se retira y no puede capturar.

La Reina no camina ni atraviesa habitaciones usando NavMesh en la versión mínima. Puede girar suavemente hacia el jugador estando visible, sin cambiar de posición. Los encuentros se activan mediante zonas y eventos narrativos; tienen un número limitado de posiciones diseñadas a mano. Como regla de justicia, nunca aparece dentro del jugador, detrás de una puerta cerrada sin ruta de salida ni en el cono de visión. Si no hay punto válido, espera y vuelve a evaluar; no salta directamente a captura. La captura solo se evalúa mientras está visible y el encuentro activo. Si desaparece, no puede capturar hasta reaparecer. El jugador puede mantenerla inmóvil mientras la mira, pero debe apartar la vista para manipular las piezas y continuar.

### Puntos de control y resolución

Crear un punto de control al entrar en las habitaciones y otro al abrir la cámara. Al reiniciar, restablecer posición segura, farol, sellos y puertas según el checkpoint; la Reina comienza inactiva y vuelve a activarse al entrar en su zona. El acertijo ya resuelto permanece resuelto al reiniciar desde el segundo checkpoint. Pausar el juego detiene temporizadores y cambios de estado. La captura no borra el progreso permanentemente.

## 7. Finales y reglas de progreso

1. **Intento de liberación:** después de recoger el documento, colocarlo en un brasero señalado mediante un soporte; activar confirmación visual y sonora, marcarlo `Destruido` y desbloquear la salida. Al cruzar, reproducir el final ambiguo de liberación.
2. **Escape con la verdad:** tras recogerlo, abrir la salida y cruzarla mientras se sostiene. Reproducir el final en el que el investigador lleva el documento consigo; el destino de la Reina sigue abierto.
3. **Atrapado:** la captura en un encuentro activo produce fundido, sonido y mensaje narrativo; desde ahí se puede reiniciar el último checkpoint o salir.

Si el jugador intenta cruzar sin el documento y sin haberlo destruido, la salida permanece bloqueada con una señal sonora y visual. No se puede seleccionar más de un final por partida. Recoger el documento, resolver el acertijo y completar un final deben ser eventos inequívocos, no depender de orden accidental de colliders.

## 8. Arquitectura mínima propuesta

| Componente propuesto | Responsabilidad |
|---|---|
| `GameFlowController` | Estados de recorrido, checkpoints, pausa y finales; coordina eventos sin buscar objetos cada frame. |
| `QueenController` | Máquina de estados, temporizadores, selección de puntos marcados y captura. |
| `QueenVisibility` | Comprueba cono, distancia y línea de visión de dos puntos; devuelve si el jugador observa a la Reina. |
| `QueenEncounterZone` | Activa/desactiva un encuentro y referencia su lista de posiciones válidas. |
| `SealPuzzleController` | Comprueba los tres soportes y emite un evento de solución una sola vez. |
| `ConstrainedDoor` | Apertura limitada, estado bloqueado y eventos de interacción. |
| `DocumentController` / `EndingTrigger` | Estado del documento, recuperación si se pierde y elección de final al salir. |
| `CheckpointController` | Restaura estados y objetos necesarios desde datos explícitos del checkpoint. |

Conectar sistemas mediante eventos de Unity o referencias serializadas desde el inspector. Usar capas para filtrar paredes y elementos de visión; validar referencias al iniciar. No acoplar la lógica del acertijo a las etiquetas `Pila 1` y `Pila 2` del prototipo anterior. Todo comportamiento debe poder probarse con geometría y personaje provisionales antes de crear el arte definitivo.

## 9. Recursos mínimos y rendimiento

- Una escena jugable con tres zonas, dos puertas funcionales, una salida, un altar, tres sellos, un farol, dos cartas, un documento, un brasero y una Reina. Añadir menú sencillo y audio ambiental.
- Para la Reina, empezar con una silueta provisional y sustituirla después por modelo y animación sencillos si están disponibles. El comportamiento debe funcionar sin animaciones complejas.
- Reutilizar módulos y recursos que ya existan, verificando que estén presentes en la copia de trabajo. Priorizar geometría estática y colisiones simples; iluminación horneada donde sea viable y una sola luz móvil ajustada al dispositivo.
- Medir en el visor objetivo con el perfilador: fotogramas estables a la frecuencia configurada del dispositivo, sin fijar todavía un número universal. Revisar resolución de texturas, memoria, materiales y complejidad del escenario antes de asumir que el conjunto de 2,2 GB rendirá en el visor.
- Audio espacial de susurros y puertas con volúmenes moderados; usar silencio y cambios de ambiente para generar tensión sin efectos costosos.

## 10. Plan de desarrollo corto y criterios de aceptación

1. **Base VR:** crear una escena de prueba con XR Origin, manos, movimiento, giro, pausa y farol. Aceptación: ambas manos agarran y sueltan; se puede completar una habitación sin atravesar paredes.
2. **Recorrido y acertijo:** montar tres zonas con formas simples, dos puertas, la palanca y tres sellos. Aceptación: combinaciones incorrectas no abren la cámara; la correcta la abre una sola vez; piezas perdidas regresan.
3. **Amenaza:** añadir Reina provisional, puntos y zonas. Aceptación: al observarla no cambia de posición; al dejar de observarla avanza tras el margen; no aparece a la vista ni dentro del jugador; captura solo en las condiciones definidas.
4. **Documento y finales:** añadir checkpoint, brasero y salida. Aceptación: destruir y salir, salir sosteniendo el documento y ser capturado producen tres resultados distintos; no se puede salir sin cumplir una condición.
5. **Presentación y validación:** reemplazar geometría provisional, añadir audio y cartas, optimizar y probar en visor. Aceptación: partida de principio a fin sin bloqueo, duración cercana a 10–15 minutos y rendimiento y comodidad aceptables para el visor escogido.

Casos a probar: girar rápido en los bordes del cono; ocultar a la Reina tras un objeto; manipular sellos con ambas manos; intentar repetir la inserción; dejar caer documento y farol fuera del mapa; abrir una puerta mientras se ocupa el umbral; pausar durante el ataque; reiniciar cada checkpoint; alternar movimiento continuo y teletransporte; completar cada final. El simulador sirve para pruebas iniciales, pero no sustituye las pruebas con el visor real.

## 11. Alcance futuro, solo si sobra tiempo

Más variantes de cartas, una reacción háptica discreta, una animación sencilla de la Reina y un segundo encuentro. No introducir sistemas de combate, múltiples enemigos, generación de pasillos o física detallada hasta tener el recorrido completo y estable.
