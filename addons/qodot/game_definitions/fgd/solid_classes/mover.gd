class_name Mover

extends CharacterBody3D

signal OnStartMove(starting_from_original)
signal OnEndMove(starting_from_original)

@export var properties: Dictionary :
	get:
		return properties # TODOConverter40 Non existent get function 
	set(new_properties):
		if(properties != new_properties):
			properties = new_properties
			update_properties()

var base_transform: Transform3D
var offset_transform: Transform3D
var target_transform: Transform3D

var CloseDelay: float = -1.0
var Togglable: bool = false

var _close_timer: float = 0.0

var speed := 1.0

var IsOpen: bool = false
var ReachedDestination: bool = false
var IsMoving: bool = false

var MoverVelocity: Vector3 = Vector3.ZERO

func update_properties() -> void:
	if 'translation' in properties:
		offset_transform.origin = properties.translation

	if 'rotation' in properties:
		offset_transform.basis = offset_transform.basis.rotated(Vector3.RIGHT, properties.rotation.x)
		offset_transform.basis = offset_transform.basis.rotated(Vector3.UP, properties.rotation.y)
		offset_transform.basis = offset_transform.basis.rotated(Vector3.FORWARD, properties.rotation.z)

	if 'scale' in properties:
		offset_transform.basis = offset_transform.basis.scaled(properties.scale)

	if 'speed' in properties:
		speed = properties.speed
	
	if 'togglable' in properties:
		Togglable = properties['togglable'] == "true"
	
	if 'close_delay' in properties:
		CloseDelay = properties['close_delay']

var _prev_pos: Vector3 = Vector3.ZERO
func _process(delta: float) -> void:
	transform = transform.interpolate_with(target_transform, speed * delta)

	if IsOpen and ReachedDestination and CloseDelay > 0.0:
		_close_timer -= delta
		if _close_timer < 0.0:
			reverse_motion()

	var EPSILON = 0.05
	var reached_pos = transform.origin.distance_squared_to(target_transform.origin) < EPSILON
	var rr_x = transform.basis.x.distance_squared_to(target_transform.basis.x) < EPSILON
	var rr_y = transform.basis.y.distance_squared_to(target_transform.basis.y) < EPSILON
	var rr_z = transform.basis.z.distance_squared_to(target_transform.basis.z) < EPSILON
	var reached_rot = rr_x and rr_y and rr_z
	if not ReachedDestination and reached_rot and reached_pos:
		ReachedDestination = true
		OnEndMove.emit(not IsOpen)
		_close_timer = CloseDelay
	
	# print("CURPOS: ", global_position, " PREVPOS: ", _prev_pos)
	MoverVelocity = (global_position - _prev_pos) / delta
	_prev_pos = global_position


func _ready() -> void:
	base_transform = transform
	target_transform = base_transform
	_close_timer = CloseDelay
	add_to_group("moving_platform")
	add_to_group("qodot_mover")

func use() -> void:
	if Togglable:
		if IsOpen:
			reverse_motion()
		else:
			play_motion()
	else:
		play_motion()
	

func play_motion() -> void:
	target_transform = base_transform * offset_transform
	IsOpen = true
	ReachedDestination = false
	OnStartMove.emit(true)

func reverse_motion() -> void:
	target_transform = base_transform
	IsOpen = false
	ReachedDestination = false
	OnStartMove.emit(false)
