# 🚀 SuperJumpPad - Configuración de Super Salto

## 📋 Descripción
Script que hace que el jugador salte súper alto cuando entra en contacto con la engrapadora (o cualquier objeto con este script).

## 🔧 Configuración en Unity

### 1. Configurar el GameObject de la Engrapadora

**En el GameObject de la engrapadora:**
1. **Agrega un Collider** (si no tiene uno)
2. **Activa "Is Trigger"** en el collider
3. **Agrega el componente "SuperJumpPad"**

### 2. Configurar el SuperJumpPad

**En el Inspector del SuperJumpPad:**
- **Jump Multiplier**: `3.0` (3 veces más alto que el salto normal)
- **Player Tag**: `"Player"` (debe coincidir con el tag del jugador)
- **Show Effects**: ✅ Activado (para efectos visuales)
- **Reset After Use**: ✅ Activado (resetea el salto cuando sales)

### 3. Configurar Efectos Visuales (Opcional)

**Partículas:**
1. Crea un GameObject hijo con ParticleSystem
2. Configura las partículas como prefieras
3. Arrastra el ParticleSystem al campo "Jump Particles"

**Sonido:**
1. Agrega un AudioSource al GameObject
2. Asigna un clip de audio
3. Arrastra el AudioSource al campo "Jump Sound"

## 🎯 Funcionamiento

### ✅ Cómo Funciona:
1. **El jugador entra** al área de la engrapadora
2. **El salto se multiplica** por el valor configurado
3. **Se activan efectos** visuales y de sonido
4. **Cuando el jugador sale**, el salto se resetea (si está configurado)

### 🔄 Dos Formas de Activación:
1. **Automática**: Al entrar al trigger
2. **Manual**: Al presionar salto mientras estás sobre la plataforma

## ⚙️ Configuración Recomendada

### Para una Engrapadora:
- **Jump Multiplier**: `2.5` - `4.0`
- **Cooldown Time**: `1.0` segundos
- **Reset After Use**: ✅ Activado

### Para una Trampolina:
- **Jump Multiplier**: `5.0` - `8.0`
- **Cooldown Time**: `0.5` segundos
- **Reset After Use**: ❌ Desactivado

## 🧪 Prueba

1. **Ejecuta la escena**
2. **Camina hacia la engrapadora**
3. **Salta** - deberías saltar mucho más alto
4. **Revisa la consola** para ver los logs:
   - "🚀 Jugador entró al SuperJumpPad"
   - "🚀 Activando super salto! Multiplicador: 3.0x"
   - "🚀 Salto modificado a: [nuevo valor]"

## 🔍 Troubleshooting

### ❌ El jugador no salta más alto:
- Verifica que el collider tenga "Is Trigger" activado
- Verifica que el jugador tenga el tag "Player"
- Verifica que el PlayerController esté asignado correctamente

### ❌ El salto no se resetea:
- Verifica que "Reset After Use" esté activado
- Verifica que el jugador salga del área del trigger

### ❌ No aparecen efectos:
- Verifica que "Show Effects" esté activado
- Verifica que las partículas y sonido estén asignados

## 📝 Logs de Debug

El script incluye logs informativos:
- 🚀 "Jugador entró al SuperJumpPad"
- 🚀 "Activando super salto! Multiplicador: 3.0x"
- ✨ "Partículas de salto activadas"
- 🔊 "Sonido de salto reproducido"
- 🔄 "Salto reseteado a valor original"

## 🎛️ Métodos Públicos

- `ActivateSuperJumpManually()`: Activa el super salto manualmente
- `SetJumpMultiplier(float)`: Cambia el multiplicador de salto
- `ResetJump()`: Resetea el salto manualmente

## 🏷️ Tags Necesarios

- **Player**: Para el GameObject del jugador
- **Grappler**: Para el GameObject de la engrapadora (opcional, solo para identificación)
