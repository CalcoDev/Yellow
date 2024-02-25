extends Node3D

@export_node_path("Node3D") var btn_path: NodePath

func _ready():
	var btn = get_node(btn_path)
	btn.connect("OnTriggerEnter", Callable(self, "ontrigenter"), CONNECT_PERSIST)
	btn.connect("OnTriggerExit", Callable(self, "ontrigexit"), CONNECT_PERSIST)
	btn.connect("OnTriggerEnterParamless", Callable(self, "ontrigenterpless"), CONNECT_PERSIST)
	btn.connect("OnTriggerExitParamless", Callable(self, "ontrigexitpless"), CONNECT_PERSIST)
	# btn.connect("trigger", Callable(self, "trig"), CONNECT_PERSIST)
	# btn.connect("OnPressed", Callable(self, "onpres"), CONNECT_PERSIST)
	# btn.connect("OnReleased", Callable(self, "onrel"), CONNECT_PERSIST)
	# btn.connect("pressed", Callable(self, "press"), CONNECT_PERSIST)
	# btn.connect("released", Callable(self, "release"), CONNECT_PERSIST)

func ontrigenter(body):
	print("ON TRIG ENTER: ", body)

func ontrigexit(body):
	print("ON TRIG EXIT: ", body)

func ontrigenterpless():
	print("ON TRIG PARAMLESS ENTER!")

func ontrigexitpless():
	print("ON TRIG PARAMLESS EXIT!")

func trig():
	print("TRIGGERED")

func onpres(body):
	print("ON PRESS: ", body)

func onrel(body):
	print("ON RELEASE: ", body)

func press():
	print("PRESS!")

func release():
	print("RELEASE!")
