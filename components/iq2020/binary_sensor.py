import esphome.codegen as cg
import esphome.config_validation as cv
from esphome.components import binary_sensor
from esphome.const import (
    DEVICE_CLASS_CONNECTIVITY,
    ENTITY_CATEGORY_DIAGNOSTIC,
)
from . import ns, IQ2020Component

CONF_SENSOR_CONNECTED = "connected"
CONF_SENSOR_LIGHTS = "lights"
CONF_SENSOR_CONNECTIONKIT = "connectionkit"
CONF_IQ2020_SERVER = "iq2020_server"

CONFIG_SCHEMA = cv.Schema(
    {
        cv.GenerateID(CONF_IQ2020_SERVER): cv.use_id(IQ2020Component),
        cv.Required(CONF_SENSOR_CONNECTED): binary_sensor.binary_sensor_schema(
            device_class=DEVICE_CLASS_CONNECTIVITY,
            entity_category=ENTITY_CATEGORY_DIAGNOSTIC,
        ),
        cv.Required(CONF_SENSOR_LIGHTS): binary_sensor.binary_sensor_schema(
            device_class=DEVICE_CLASS_SWITCH,
            entity_category=ENTITY_CATEGORY_NONE,
            icon=ICON_LIGHTBULB,
        ),
        cv.Required(CONF_SENSOR_CONNECTIONKIT): binary_sensor.binary_sensor_schema(
            device_class=DEVICE_CLASS_PRESENCE,
            entity_category=ENTITY_CATEGORY_DIAGNOSTIC,
        ),
    }
)


async def to_code(config):
    server = await cg.get_variable(config[CONF_IQ2020_SERVER])

#    sens = await binary_sensor.new_binary_sensor(config[CONF_SENSOR_CONNECTED])
#    cg.add(server.set_connected_sensor(sens))

	if CONF_SENSOR_CONNECTED in config:
        sens = yield binary_sensor.new_binary_sensor(config[CONF_SENSOR_CONNECTED])
        cg.add(var.set_connected_sensor(sens))
        
    if CONF_SENSOR_LIGHTS in config:
        sens = yield binary_sensor.new_binary_sensor(config[CONF_SENSOR_LIGHTS])
        cg.add(var.set_lights_sensor(sens))
        
    if CONF_SENSOR_CONNECTIONKIT in config:
        sens = yield binary_sensor.new_binary_sensor(config[CONF_SENSOR_CONNECTIONKIT])
        cg.add(var.set_connectionkit_sensor(sens))