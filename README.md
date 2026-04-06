# Horror Courier Simulator

Unity 6 (HDRP) | New Input System | C# 9

---

## Architecture Overview

Проект использует **Component-Based Architecture** с паттерном **Service Locator** для глобальных систем.

### Почему именно эта архитектура?

Для хоррор-игры с симулятором курьера нужна модульность: системы доставки, хоррор-события, инвентарь, диалоги - всё это независимые модули, которые должны легко добавляться и не ломать друг друга. Компонентный подход Unity идеально подходит.

---

## Diagram: System Layers

```
┌─────────────────────────────────────────────────────────┐
│                      GAME LAYER                         │
│  GameManager  ·  SceneLoader  ·  SaveSystem             │
│  (глобальное состояние, переходы, сохранения)           │
└───────────────────────┬─────────────────────────────────┘
                        │ управляет
┌───────────────────────▼─────────────────────────────────┐
│                   GAMEPLAY SYSTEMS                       │
│                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │  Delivery    │  │   Horror     │  │  Inventory    │  │
│  │  System      │  │   System     │  │  System       │  │
│  │              │  │              │  │               │  │
│  │ - маршруты   │  │ - события    │  │ - посылки     │  │
│  │ - таймеры    │  │ - триггеры   │  │ - предметы    │  │
│  │ - рейтинг    │  │ - атмосфера  │  │ - руки/слоты  │  │
│  └──────────────┘  └──────────────┘  └───────────────┘  │
│                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │  Dialogue    │  │    AI /      │  │   Audio       │  │
│  │  System      │  │   NPC        │  │   System      │  │
│  │              │  │   System     │  │               │  │
│  │ - реплики    │  │ - патрули    │  │ - эмбиент     │  │
│  │ - выборы     │  │ - поведение  │  │ - sfx         │  │
│  │ - квесты     │  │ - detection  │  │ - музыка      │  │
│  └──────────────┘  └──────────────┘  └───────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ использует
┌───────────────────────▼─────────────────────────────────┐
│                    PLAYER LAYER                          │
│                                                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │                  Player (GameObject)              │   │
│  │                                                   │   │
│  │  PlayerMovement ──── CharacterController          │   │
│  │  (walk / sprint / stamina)                        │   │
│  │                                                   │   │
│  │  └─ Camera (child)                                │   │
│  │     FirstPersonCamera                             │   │
│  │     (mouse look, pitch clamp)                     │   │
│  │                                                   │   │
│  │  PlayerInteraction (raycast → IInteractable)      │   │
│  │  PlayerInventory (slots, held item)               │   │
│  │  PlayerHealth / Sanity                            │   │
│  └──────────────────────────────────────────────────┘   │
└───────────────────────┬─────────────────────────────────┘
                        │ читает инпут из
┌───────────────────────▼─────────────────────────────────┐
│                    INPUT LAYER                           │
│                                                         │
│  InputManager (Singleton, DontDestroyOnLoad)            │
│  ├── Player Action Map:  Move, Look, Sprint, Interact,  │
│  │                       Inventory, Pause               │
│  └── UI Action Map:     Navigate, Submit, Cancel, ...   │
│                                                         │
│  Переключение карт: InputManager.ChangeInputMap()       │
└─────────────────────────────────────────────────────────┘
```

---

## Folder Structure (target)

```
Assets/
├── Script/
│   ├── Core/               # InputManager, GameManager, SceneLoader
│   ├── Player/             # PlayerMovement, FirstPersonCamera, PlayerInteraction
│   ├── Systems/            # DeliverySystem, HorrorSystem, InventorySystem
│   ├── AI/                 # NPC behaviour, patrol, detection
│   ├── UI/                 # HUD, menus, dialogue UI
│   ├── Interactables/      # IInteractable, Door, Pickup, DeliveryPoint
│   └── Data/               # ScriptableObjects (items, routes, events)
├── Prefabs/
├── Scenes/
├── Audio/
├── Materials/
├── Models/
└── Settings/               # HDRP assets
```

---

## Key Patterns

### 1. Service Locator (InputManager)
Глобальные системы доступны через `Instance`. Используется только для систем, которые действительно нужны отовсюду (Input, Audio, GameState). Не злоупотреблять.

### 2. Interface-based Interaction
```csharp
public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(PlayerInteraction player);
}
```
Двери, посылки, NPC - всё реализует `IInteractable`. PlayerInteraction делает Raycast и вызывает `Interact()`.

### 3. ScriptableObjects for Data
Предметы, маршруты доставки, хоррор-события - всё хранится в ScriptableObject. Это позволяет геймдизайнерам настраивать данные без кода.

### 4. Event-Driven Communication
Системы общаются через C# events / UnityEvents, а не прямые ссылки:
```csharp
// DeliverySystem
public event System.Action<DeliveryData> OnDeliveryCompleted;

// HorrorSystem подписывается
DeliverySystem.OnDeliveryCompleted += TriggerHorrorEvent;
```

---

## Current State

### Implemented
- **InputManager** - singleton, переключение карт Player/UI
- **PlayerMovement** - ходьба (WASD), бег (Shift), стамина с задержкой регенерации
- **FirstPersonCamera** - обзор мышью, clamp по вертикали

### Next Steps
- [ ] PlayerInteraction (raycast + IInteractable)
- [ ] Inventory system (hold/drop packages)
- [ ] Delivery system (routes, timers, scoring)
- [ ] Horror event system (triggers, atmosphere)
- [ ] AI / NPC system
- [ ] UI (HUD: stamina bar, delivery info, interaction prompt)
- [ ] Save/Load system
- [ ] Audio manager (ambient, SFX layers)

---

## Scene Setup

Для работы текущего кода нужен следующий GameObject:

```
Player (GameObject)
├── Component: CharacterController
├── Component: PlayerMovement
│
└── Camera (Child GameObject)
    ├── Component: Camera
    └── Component: FirstPersonCamera
        └── playerBody → ссылка на Player (parent)
```

1. Создай пустой GameObject "Player", добавь `CharacterController` и `PlayerMovement`
2. Сделай Main Camera дочерним объектом Player, добавь `FirstPersonCamera`
3. В инспекторе `FirstPersonCamera` перетащи Player в поле `playerBody`
4. `InputManager` создается автоматически (RuntimeInitializeOnLoadMethod)
