# Prompt: mecánica de la Reina del Silencio en VR

Trabaja en el proyecto Unity `E:/ProyectosUnity/HALLOWEEN-RV-REPO`, en la escena `Assets/Game/Scenes/SceneTest.unity`. Antes de editar, inspecciona la escena, el estado de Git y los scripts existentes; conserva los cambios del usuario y evita incluir modificaciones ajenas a esta tarea.

## Objetivo

Implementa y configura una primera versión sencilla de la Reina del Silencio para probarla en el fragmento de pasillo que ya está en `SceneTest`. Usa el modelo de la Reina que ya se encuentra en la escena y el `XR Origin` existente. La experiencia debe priorizar el movimiento de cabeza del jugador en VR y permitirle continuar recogiendo y encajando los fragmentos del bebé.

La decisión más reciente para esta mecánica sustituye la propuesta anterior de desplazamiento mediante puntos preparados: utiliza **NavMesh y movimiento continuo**, sin exigir la colocación manual de empties para cada aparición. El documento técnico anterior puede servir como contexto para las demás reglas, pero no mantengas su restricción de usar puntos fijos para el movimiento de la Reina.

## Comportamiento solicitado

1. La Reina debe desplazarse por las zonas transitables del pasillo y las habitaciones mediante `NavMeshAgent`, sin atravesar las paredes ni abandonar la superficie navegable. Configura y hornea un NavMesh apropiado para el fragmento de prueba; revisa el tamaño del agente, la continuidad de la superficie y las rutas estrechas.
2. Mientras el jugador observe a la Reina, ella permanece inmóvil. Utiliza la orientación y posición de la cámara del visor VR, un ángulo y una distancia de visión razonables, y una comprobación de línea de visión contra obstáculos. No consideres observada a la Reina si una pared bloquea la vista; evita que sus propios colliders o los del jugador den resultados falsos. Considera cabeza y torso si el modelo lo permite.
3. Cuando el jugador deje de verla durante un pequeño margen configurable, la Reina comienza a avanzar hacia él siguiendo el NavMesh. Si vuelve a mirarla, se detiene. Actualiza el destino de forma razonable, sin recalcular innecesariamente en cada frame. Usa una velocidad inicial ajustable para probarla; evita efectos de movimiento de cámara que puedan incomodar en VR.
4. La Reina, como espíritu, debe poder **atravesar puertas cerradas**, pero no paredes ni otros elementos que delimiten las zonas transitables. Ajusta la navegación y las colisiones de forma específica para la Reina, sin desactivar las colisiones del jugador con las puertas ni romper las mecánicas existentes. Comprueba que el NavMesh conecte ambos lados de las puertas y que exista una ruta válida cuando están cerradas. No te limites a ignorar una colisión física si la puerta sigue bloqueando el cálculo de ruta.
5. Cuando la Reina esté visible y lo suficientemente cerca del jugador durante un breve periodo continuo, activa una respuesta de captura provisional claramente reconocible. El umbral inicial puede ser 1,2 m durante 1 segundo, ajustable tras la prueba. No inventes un sistema completo de muerte, menú o checkpoints para esta primera versión.
6. La ofrenda del altar debe detener definitivamente la amenaza durante esa partida y retirar a la Reina. Conserva la conexión y el comportamiento existentes de `AltarController`, incluidos el tiempo de confirmación configurado actualmente, las piezas, la luz gradual, el audio y la apertura de salida. Evita que una captura y la ofrenda se activen a la vez.

## Restricciones de implementación

- Mantén el alcance en una sola Reina y el fragmento de prueba. No añadas combate, inventario, navegación procedural, teletransporte de la Reina, múltiples enemigos ni sistemas ajenos a esta mecánica.
- Usa el paquete AI Navigation ya instalado en el proyecto. Implementa solo el código y las referencias necesarios, con nombres claros; no añadas tooltips ni variables innecesarias.
- La detección del jugador debe funcionar con la cámara del `XR Origin`, no con una cámara independiente de prueba.
- Si no hay NavMesh válido, el agente no puede llegar al objetivo o falta una referencia, evita excepciones y no desplaces a la Reina a través de las paredes. Explica cualquier configuración pendiente.
- Conserva las posiciones, materiales, prefabs y cambios del usuario. No modifiques la escena principal ni otras mecánicas salvo lo estrictamente necesario para la prueba.
- Si el editor está abierto, preserva su estado y sus cambios sin guardar. No hagas commit ni push salvo que se solicite expresamente.

## Verificación

Comprueba en Unity, mediante pruebas apropiadas y sin afirmar que se probó en visor si no se ha hecho:

- La Reina se detiene al ser observada y vuelve a avanzar tras el margen cuando el jugador aparta la vista.
- Una pared bloquea la visión; un pequeño giro o una oclusión breve no genera cambios bruscos.
- Puede seguir una ruta entre ambos lados de una puerta cerrada sin atravesar paredes, y el jugador sigue colisionando con la puerta.
- Respeta los límites del NavMesh y no intenta continuar si la ruta es inválida.
- La captura provisional requiere la cercanía durante el tiempo indicado; al completar la ofrenda no vuelve a perseguir ni a capturar.
- El farol, las puertas, los tres sockets y la resolución del altar siguen funcionando.

Al terminar, indica qué scripts y objetos se configuraron, cómo probar la mecánica en `SceneTest`, las pruebas realizadas y sus limitaciones, especialmente si falta una prueba con visor VR.
