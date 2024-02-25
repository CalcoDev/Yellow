class_name QodotButton
extends Area3D

signal trigger()
signal pressed()
signal released()

signal OnTriggerEnter(body)
signal OnTriggerExit(body)

signal OnTriggerEnterParamless()
signal OnTriggerExitParamless()

signal OnPressed(body)
signal OnReleased(body)

@export var properties: Dictionary :
	get:
		return properties # TODOConverter40 Non existent get function 
	set(new_properties):
		if(properties != new_properties):
			properties = new_properties
			update_properties()

var is_pressed = false
var base_translation = Vector3.ZERO
var axis := Vector3.DOWN
var speed := 8.0
var depth := 0.8
var release_delay := 0.0
var trigger_signal_delay :=  0.0
var press_signal_delay :=  0.0
var release_signal_delay :=  0.0

var _layer_mask: int = 0

var overlaps := 0

func update_properties() -> void:
	if 'axis' in properties:
		axis = properties.axis.normalized()

	if 'speed' in properties:
		speed = properties.speed

	if 'depth' in properties:
		depth = float(properties.depth)

	if 'release_delay' in properties:
		release_delay = properties.release_delay

	if 'trigger_signal_delay' in properties:
		trigger_signal_delay = properties.trigger_signal_delay

	if 'press_signal_delay' in properties:
		press_signal_delay = properties.press_signal_delay

	if 'release_signal_delay' in properties:
		release_signal_delay = properties.release_signal_delay
	
	if 'layer_mask' in properties:
		_layer_mask = _compute_layer_mask()
	else:
		_layer_mask = 0
	collision_mask = _layer_mask

func _compute_layer_mask() -> int:
	if 'layer_mask' in properties:
		var layers = _parse_layers()
		var lmask: int = 0
		for layer in properties['layer_mask'].split(","):
			var trimmed: String = layer.trim_prefix(" ").trim_suffix(" ").to_lower()
			if trimmed in layers:
				lmask += 1 << layers[trimmed]
		return lmask
	else:
		return 0

# TODO(calco): This should just be static but gdscript no do that :skull:
func _parse_layers() -> Dictionary:
	var dict: Dictionary
	for i in 33:
		var name: String = ProjectSettings.get_setting("layer_names/3d_physics/layer_%d" % i, "NO_NAME")
		dict[name.to_lower()] = i - 1;
	return dict;


func _init() -> void:
	connect("body_shape_entered", body_shape_entered)
	connect("body_shape_exited", body_shape_exited)

func _enter_tree() -> void:
	base_translation = position

func _process(delta: float) -> void:
	var target_position = base_translation + (axis * (depth if is_pressed else 0.0))
	position = position.lerp(target_position, speed * delta)

func body_shape_entered(body_id, body: Node, body_shape_idx: int, self_shape_idx: int) -> void:
	if body is StaticBody3D:
		return

	OnTriggerEnterParamless.emit()
	OnTriggerEnter.emit(body)
	if overlaps == 0:
		press(body)

	overlaps += 1

func body_shape_exited(body_id, body: Node, body_shape_idx: int, self_shape_idx: int) -> void:
	if body is StaticBody3D:
		return

	overlaps -= 1
	OnTriggerExit.emit(body)
	OnTriggerEnterParamless.emit()
	if overlaps == 0:
		if release_delay == 0:
			release(body)
		elif release_delay > 0:
			await get_tree().create_timer(release_delay).timeout
			release(body)

func press(body) -> void:
	if is_pressed:
		return

	is_pressed = true

	emit_trigger(body)
	emit_pressed(body)

func emit_trigger(body) -> void:
	await get_tree().create_timer(trigger_signal_delay).timeout
	trigger.emit()

func emit_pressed(body) -> void:
	await get_tree().create_timer(press_signal_delay).timeout
	pressed.emit()
	OnPressed.emit(body)

func release(body) -> void:
	if not is_pressed:
		return

	is_pressed = false

	await get_tree().create_timer(release_delay).timeout
	released.emit()
	OnReleased.emit(body)
