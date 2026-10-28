# El último sueño de Daejojeon — Especificaciones técnicas y mecánicas VR

**Estado:** diseño actualizado del objetivo narrativo y del ensamblaje mediante sockets. Esta nueva mecánica todavía debe implementarse; se reutilizan las interacciones VR existentes.  
**Referencia narrativa:** `historia-base.md`.  
**Experiencia objetivo:** terror psicológico para una persona, de 10 a 15 minutos, en tres espacios conectados.  
**Prioridad:** terminar una experiencia completa, comprensible y cómoda con el menor número posible de sistemas y recursos nuevos.

## 1. Idea central

El objetivo principal es **encontrar tres fragmentos de la estatua del bebé difunto y petrificado de la Reina, reconstruirla mediante sockets en el altar de su habitación y completar la ofrenda para abrir la salida**. El montaje y la entrega son el mismo acto: las piezas permanecen en el altar.

El jugador usa sus manos para transportar, inspeccionar y encajar los fragmentos mientras vigila a la Reina del Silencio. Cuando la observa, ella permanece inmóvil; cuando aparta la vista durante el tiempo suficiente, desaparece y puede reaparecer más cerca. La tensión nace de tener que mirar las piezas, las manos y los puntos de encaje sin perder de vista a la amenaza.

No hay combate, armas, inventario complejo, enemigos adicionales ni persecución con navegación libre en la versión inicial. La Reina se desplaza entre posiciones preparadas de antemano. El cambio de los pasillos se representa con puertas, iluminación y sonido, sin generación procedural.

El alcance nuevo se limita a la búsqueda de fragmentos, su montaje y la ofrenda. Se conservan el farol, la locomoción, las puertas, las cartas como pistas, la palanca, los botones opcionales, las reglas de mirada de la Reina, la captura, la pausa y los puntos de control.

## 2. Base técnica existente y supuestos

- El repositorio declara Unity **6000.5.6f1**, Universal Render Pipeline 17.5.0, Input System 1.20.0, XR Interaction Toolkit 3.5.1, XR Management 4.5.4 y OpenXR 1.17.1.
- Incluye los Starter Assets y el XR Device Simulator del XR Interaction Toolkit; también existen scripts `XRLever` y `XRPushButton` en `Assets/UI_3D/UI_3D/Scripts`, cuya integración y compatibilidad se deben comprobar en una escena de prueba.
- El ensamblaje se basará expresamente en `Assets/Game/Scripts/TagSocket.cs`, que hereda de `XRSocketInteractor` y ya filtra objetos por `targetTag` en `CanHover` y `CanSelect`. Sus notificaciones actuales están ligadas a `Pila 1` y `Pila 2`; requieren la adaptación descrita en la sección 5.5. El `InteractionManager` existente puede reutilizar sus tres indicadores independientes, pero sus mensajes actuales no completan una ofrenda.
- Se propone usar dos mandos VR con agarre, gatillo y palancas analógicas. El dispositivo objetivo y su presupuesto de rendimiento aún están por definir. El seguimiento de manos y ojos no es necesario.
- **Estado local observado:** `Assets/DaejojeonComplex` aparece eliminado en la copia de trabajo y existen otros cambios sin confirmar. Antes de montar el nivel hay que comprobar con el equipo si el escenario fue movido o retirado; no restaurar ni sobrescribir cambios sin entenderlos.
- Valores como tiempos, distancias y velocidades son puntos de partida para pruebas, no resultados medidos.

## 3. Recorrido jugable

| Momento | Tiempo orientativo | Acción y resultado |
|---|---:|---|
| Entrada y aprendizaje | 1–2 min | Moverse, tomar el farol, abrir una puerta y encontrar la primera carta. No hay captura. |
| Corredor | 2–3 min | Primera aparición de la Reina y aprendizaje de su comportamiento. Una pista introduce la búsqueda de las piezas. |
| Sala auxiliar y búsqueda | 3–4 min | Explorar los espacios cercanos y encontrar los tres fragmentos en lugares fijos y accesibles. Se pueden entregar de uno en uno. |
| Habitación de la Reina | 3–4 min | Reconocer el altar, accionar la palanca y encajar las piezas con TagSocket. La vigilancia y el montaje generan la tensión principal. |
| Ofrenda y salida | 1–2 min | La estatua completa sobre el altar confirma la ofrenda, desactiva el acecho y abre la salida. Cruzar el umbral completa el final. |

Los tiempos se ajustan con pruebas para aproximarse a 10–15 minutos. Corredor, sala auxiliar y habitación de la Reina forman las tres zonas principales; la entrada es un tramo protegido del corredor. La habitación y el altar deben poder visitarse antes de reunir todas las piezas. No colocar una pieza detrás de una puerta que exija completar la propia estatua.

La búsqueda permite recorridos cortos de ida y vuelta, con las piezas a pocos metros del altar. Se puede llevar el farol en una mano y un fragmento en la otra, o apoyar el farol en una superficie para usar ambas manos. No se exige transportar tres piezas a la vez ni añadir un inventario.

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

### 5.3. Cartas y pistas

- Se conservan dos cartas breves como objetos agarrables con `XR Grab Interactable`, collider y `Rigidbody`.
- Acercar y girar una carta permite leer texto grande, con contraste suficiente. La primera orienta al jugador hacia la búsqueda; la segunda explica la pérdida de la Reina y muestra dónde deben encajar los fragmentos en el altar.
- Las cartas apoyan la comprensión, pero leerlas no es una condición técnica de progreso. El jugador puede resolver el montaje por la forma de las piezas y las señales del altar.
- El progreso depende de los fragmentos colocados en sus sockets y de la confirmación de la ofrenda. No se necesita inventario ni un objeto adicional que llevar a la salida.

### 5.4. Puertas

Se conserva la mecánica ya implementada de las puertas deslizantes Door08: agarrar una hoja y tirar una distancia corta activa el recorrido completo de ambas hojas; tirar en sentido inverso las cierra. Se mantienen `SlidingDoorGroup`, `SlidingDoorHandle`, los prefabs dobles, las colisiones y la pausa del movimiento cuando el cuerpo del jugador ocupa el recorrido. La configuración y los prefabs se describen en `puertas-door08.md`.

- Puertas normales y de la sala auxiliar: permiten explorar y regresar al altar; no dependen de haber reunido las piezas.
- Puerta de la habitación de la Reina: debe permitir llegar al altar durante la búsqueda. No bloquearla hasta completar el montaje.
- Puerta final: como regla de progreso pendiente de implementar, se desbloquea al confirmar la ofrenda completa. Su bloqueo no modifica el gesto de agarre y apertura existente.
- Los cierres sobrenaturales se ejecutan únicamente en transiciones definidas y cuando el jugador no ocupa el umbral. No cerrar puertas sobre el jugador, el farol ni los fragmentos.

### 5.5. Reconstrucción del bebé petrificado con TagSocket

**Decisión técnica:** reutilizar `TagSocket.cs` y los sockets de XRI existentes. El único acertijo principal será encontrar y colocar tres fragmentos de piedra en tres sockets fijos que, juntos, forman la estatua sobre el altar de la habitación de la Reina. No se requiere pegado físico, joints entre fragmentos, detección de fracturas ni una animación para ensamblarlos.

#### Piezas y distribución

La propuesta inicial usa tres piezas reconocibles: parte inferior/base de la figura, torso envuelto en una manta y cabeza. Son fragmentos de una figura pétrea; su división definitiva se ajustará al modelo disponible, manteniendo tres piezas para limitar el trabajo.

Cada pieza tendrá `XR Grab Interactable`, un `Rigidbody` y un collider sencillo. Puede recogerse con cualquiera de las manos, girarse para inspeccionarla, apoyarse en superficies y volver a recogerse. Las piezas se distribuyen en puntos fijos del corredor y la sala auxiliar, con caminos cortos hacia el altar. Una pieza caída fuera del mapa vuelve a un punto seguro y accesible; no debe reaparecer detrás de una puerta que vuelva a bloquearla.

#### Sockets y etiquetas

Los tres sockets son objetos hijos de una misma zona de montaje del altar. Su posición y orientación representan las posiciones finales de los fragmentos. Los tres pueden estar disponibles a la vez después de accionar la palanca; no se añade un segundo acertijo de orden.

| Fragmento | Tag en el objeto que contiene XR Grab Interactable | targetTag del TagSocket | Indicador del controlador existente |
|---|---|---|---|
| Parte inferior/base | `BebeBase` | `BebeBase` | `panel1` |
| Torso | `BebeTorso` | `BebeTorso` | `panel2` |
| Cabeza | `BebeCabeza` | `BebeCabeza` | `panel3` |

Las etiquetas propuestas deben crearse en el Tag Manager y escribirse exactamente igual, respetando mayúsculas. Deben asignarse al mismo GameObject que identifica el interactuable: el script consulta `interactable.transform.CompareTag(targetTag)`, no una etiqueta arbitraria de su malla hija.

Cada socket utiliza una zona de detección cómoda, inicialmente de unos 10–15 cm, y un Attach Transform ajustado para alinear el fragmento con el modelo final. El jugador acerca la pieza correcta y la suelta; el socket la ajusta a su posición y orientación. No exigir precisión milimétrica ni alineación angular estricta para la primera versión. Una pieza con otra etiqueta no obtiene hover válido ni se acopla y continúa disponible para agarrarla.

#### Qué hace hoy TagSocket y qué debe adaptarse

- `CanHover` y `CanSelect` ya ofrecen el filtro por etiqueta necesario. Mantener esa base.
- `OnSelectEntered` ya llama a `ActivarPanel` y `OnSelectExited` a `DesactivarPanel`. Reutilizar esos puntos de notificación para actualizar el estado del montaje.
- Los switch actuales solo contemplan `Pila 1` y `Pila 2`. En el montaje nuevo deben reconocer las tres etiquetas de la tabla y activar o desactivar únicamente el indicador correspondiente.
- Corregir para este uso la lógica que actualmente hace que `Pila 2` cambie simultáneamente `panel2` y `panel3`. Cada fragmento del bebé representa un solo indicador; dos piezas nunca pueden completar los tres estados.
- Debe existir un `InteractionManager` válido en la escena antes de insertar piezas. La adaptación debe validar su referencia para evitar una excepción si falta.
- El método actual `isPanelActive` únicamente cambia `panelBotonApagado` e imprime mensajes. Se debe añadir una notificación de montaje completo basada en la transición de los tres indicadores a verdadero. `panelBotonApagado` por sí solo no equivale a una ofrenda confirmada.

**Cambiar solamente targetTag no basta:** el filtrado funcionaría, pero los switch actuales no registrarían las nuevas piezas. Esta adaptación se especifica aquí como trabajo pendiente; el cambio de diseño no supone que los scripts ya hayan sido modificados.

#### Montaje, extracción y confirmación de la ofrenda

Antes de confirmar la ofrenda, las piezas encajadas pueden retirarse. `OnSelectExited` debe desactivar solo su indicador, y el conjunto vuelve a considerarse incompleto. Se necesita verificar la ocupación real de los tres sockets, no solo haber acercado o recogido las piezas alguna vez.

Cuando los tres sockets contienen sus piezas correctas y ninguna sigue seleccionada por una mano, se mantiene la condición durante aproximadamente 0,5 s y se confirma la ofrenda una sola vez. La confirmación debe estar cancelada si se retira una pieza durante ese margen.

Al confirmarse: reproducir sonido y un cambio de luz discreto, marcar `ofrendaConfirmada`, impedir que el jugador desmonte la figura, desactivar el encuentro de la Reina y desbloquear la salida. El bloqueo de agarre debe conservar la selección de los sockets; no desactivar sin más TagSocket o los interactuables si eso provoca eventos de extracción. El estado final queda registrado aunque se repita un callback.

La estatua permanece reconstruida en el único altar, dentro de la habitación de la Reina. No se sustituye por un objeto que haya que volver a transportar ni se introduce otra estación de entrega. Así, la búsqueda y el montaje aprovechan las manos de VR con los componentes que ya existen.

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

Se mantienen puntos de control en memoria durante la partida: uno al entrar en las habitaciones y otro después de confirmar la ofrenda. Conservar el avance de las piezas que ya estaban encajadas y el estado de la palanca para no repetir toda la búsqueda después de una captura.

Al reiniciar, restablecer posición segura, farol, puertas y fragmentos según ese progreso. Las piezas que estaban en la mano vuelven a una superficie accesible; las ya montadas se restauran en sus sockets y las restantes en sus puntos disponibles. Restablecer los indicadores del InteractionManager de acuerdo con la ocupación real de los sockets, no dejarlos verdaderos sin la pieza física.

La Reina comienza inactiva y vuelve a activarse al entrar en su zona si la ofrenda no está confirmada. Después de la ofrenda, restaurar la estatua completa, mantener la salida desbloqueada y no reactivar la captura. Pausar el juego detiene temporizadores y cambios de estado. La captura no borra el progreso permanentemente; no se exige guardado a disco en esta versión corta.

## 7. Finales y reglas de progreso

1. **Ofrenda y descanso:** reconstruir la estatua en los tres sockets del altar de la habitación de la Reina confirma la entrega. La entidad deja de perseguir al jugador y se desbloquea la salida. Al cruzar su umbral se reproduce el final ambiguo: la Reina parece descansar, pero no se garantiza que el ciclo haya terminado para siempre.
2. **Atrapado:** la captura en un encuentro activo produce fundido, sonido y mensaje narrativo; desde ahí se puede reiniciar el último checkpoint o salir de la experiencia.

Estados del objetivo: `BuscandoFragmentos` → `MontajeParcial` → `OfrendaConfirmada` → `SalidaCompletada`. Antes de confirmar la ofrenda, retirar una pieza revierte el montaje a parcial. Después de confirmarla, la figura queda fijada en el altar.

La salida permanece bloqueada mientras `ofrendaConfirmada` sea falso. Encontrar las tres piezas, sostenerlas o rozar los sockets no completa el objetivo. Ningún final puede dispararse más de una vez; el controlador de progreso resuelve la transición de forma explícita y evita que ofrenda y captura se registren a la vez.

## 8. Arquitectura mínima propuesta

| Componente | Responsabilidad |
|---|---|
| `GameFlowController` — propuesto | Estados de recorrido, checkpoints, pausa, salida y desenlaces. |
| `QueenController` — propuesto | Máquina de estados, temporizadores, selección de puntos marcados y captura; conservar su diseño. |
| `QueenVisibility` — propuesto | Cono, distancia y línea de visión de la Reina; conservar su diseño. |
| `QueenEncounterZone` — propuesto | Activar o desactivar encuentros en zonas definidas. |
| `TagSocket` — existente, requiere adaptación | Filtrar por targetTag y notificar la inserción o extracción de cada fragmento con un estado independiente. |
| `InteractionManager` — existente, requiere adaptación | Registrar panel1/base, panel2/torso y panel3/cabeza; emitir el cambio de montaje completo sin depender solo de mensajes de consola. |
| `AltarController` — propuesto | Validar ocupación estable y liberación de las manos; confirmar la ofrenda una vez, fijar la figura y avisar a la Reina y la salida. |
| Recuperación de fragmentos — propuesta | Volver a situar piezas perdidas en un punto seguro, respetando los checkpoints y evitando duplicados. |
| `SlidingDoorGroup` / `SlidingDoorHandle` — existentes | Mantener el gesto de agarre, apertura/cierre automático y protección del jugador de las Door08. |
| `CheckpointController` — propuesto | Restaurar posición, piezas, sockets, palanca y ofrenda según el progreso de la partida. |

Conectar sistemas mediante eventos de Unity o referencias serializadas desde el inspector. Usar capas para filtrar paredes y elementos de visión; validar referencias al iniciar. El nuevo montaje utiliza las etiquetas del bebé, sin reutilizar las etiquetas de pilas. Todo comportamiento debe poder probarse con tres formas provisionales antes de terminar el arte.

Jerarquía propuesta para el único altar:

```text
HabitacionReina
└── AltarReina
    ├── Superficie y collider
    ├── Palanca y panel de los soportes
    ├── ZonaMontaje
    │   ├── SocketBase   [TagSocket: BebeBase]
    │   ├── SocketTorso  [TagSocket: BebeTorso]
    │   └── SocketCabeza [TagSocket: BebeCabeza]
    ├── AltarController
    └── Audio y señal de confirmación
```

Los sockets son puntos fijos del montaje, no nuevos objetos agarrables. Cada fragmento tiene un único objeto interactuable y aparece una sola vez en el nivel.

## 9. Recursos mínimos y rendimiento

- Una escena jugable con tres zonas, las puertas interactivas del recorrido y una salida, un único altar en la habitación de la Reina, tres fragmentos de una estatua, tres TagSocket, una palanca, un farol, dos cartas y una Reina. Reutilizar los prefabs Door08 existentes. Añadir menú sencillo y audio ambiental.
- Para la Reina, empezar con una silueta provisional y sustituirla después por modelo y animación sencillos si están disponibles. El comportamiento debe funcionar sin animaciones complejas.
- Reutilizar módulos y recursos que ya existan, verificando que estén presentes en la copia de trabajo. Priorizar geometría estática y colisiones simples; iluminación horneada donde sea viable y una sola luz móvil ajustada al dispositivo.
- Medir en el visor objetivo con el perfilador: fotogramas estables a la frecuencia configurada del dispositivo, sin fijar todavía un número universal. Revisar resolución de texturas, memoria, materiales y complejidad del escenario antes de asumir que el conjunto de 2,2 GB rendirá en el visor.
- Audio espacial de susurros y puertas con volúmenes moderados; usar silencio y cambios de ambiente para generar tensión sin efectos costosos.

## 10. Plan de desarrollo corto y criterios de aceptación

1. **Base VR:** conservar y verificar XR Origin, manos, movimiento, giro, pausa y farol. Aceptación: ambas manos agarran y sueltan; se puede completar una habitación sin atravesar paredes.
2. **Recorrido y montaje con sockets:** distribuir tres fragmentos, mantener las puertas, colocar la palanca y los tres TagSocket del altar. Adaptar las etiquetas y los indicadores del controlador existente. Aceptación: cada socket acepta solo su pieza; la base, el torso y la cabeza cambian tres indicadores diferentes; una pieza incorrecta o solo dos piezas no completan la figura.
3. **Amenaza:** añadir Reina provisional, puntos y zonas conservando el comportamiento diseñado. Aceptación: al observarla no cambia de posición; al dejar de observarla avanza tras el margen; no aparece a la vista ni dentro del jugador; captura solo en las condiciones definidas.
4. **Ofrenda, checkpoints y salida:** conectar la confirmación del altar con la retirada de la Reina y el desbloqueo de la salida. Aceptación: retirar una pieza antes de completar cancela la confirmación; las tres piezas correctas y liberadas confirman una sola vez; la salida no abre antes; captura y escape producen resultados distintos.
5. **Presentación y validación:** sustituir formas provisionales por los fragmentos de piedra, añadir audio y cartas, optimizar y probar en visor. Aceptación: partida de principio a fin sin bloqueo, duración cercana a 10–15 minutos y rendimiento y comodidad aceptables para el visor escogido.

Casos a probar: girar rápido en los bordes del cono; ocultar a la Reina tras un objeto; manipular fragmentos con ambas manos; intentar insertar una pieza con tag incorrecto; configurar un targetTag inexistente y detectarlo antes de jugar; retirar cada pieza antes de la ofrenda y comprobar que solo cae su indicador; comprobar que la lógica antigua de Pila 2 no completa dos estados; mantener una pieza agarrada dentro del socket y comprobar que todavía no confirma; repetir callbacks sin duplicar la ofrenda; dejar caer piezas y farol fuera del mapa; abrir una puerta con el umbral ocupado; pausar durante el ataque o el margen de confirmación; reiniciar cada checkpoint; alternar movimiento continuo y teletransporte; completar el escape y el desenlace de captura. El simulador sirve para pruebas iniciales, pero no sustituye las pruebas con el visor real.

## 11. Alcance futuro, solo si sobra tiempo

Más variantes de cartas, una reacción háptica discreta, una animación sencilla de la Reina y un segundo encuentro. No introducir sistemas de combate, múltiples enemigos, generación de pasillos o física detallada hasta tener el recorrido completo y estable.
