import esphome.codegen as cg
import esphome.config_validation as cv
from esphome.components import fan, output
from esphome.const import CONF_ID
#from .. import iq2020_fan_ns

from . import ns, IQ2020Component

CONF_FAN_DATAPOINT = "fan_datapoint"

#IQ2020Fan = iq2020_fan_ns.class_('iq2020_fan', cg.Component)

iq2020_fan_ns = cg.esphome_ns.namespace('iq2020_fan')
IQ2020Fan = iq2020_fan_ns.class_('IQ2020Fan', fan.Fan, cg.Component)

CONFIG_SCHEMA = fan.FAN_SCHEMA.extend({
    cv.GenerateID(CONF_ID): cv.declare_id(IQ2020Fan),
#    cv.Required(CONF_OUTPUT): cv.use_id(output.BinaryOutput),
#    cv.Optional(CONF_DIRECTION_OUTPUT): cv.use_id(output.BinaryOutput),
#    cv.Optional(CONF_OSCILLATION_OUTPUT): cv.use_id(output.BinaryOutput),
}).extend({ cv.Required(CONF_FAN_DATAPOINT): cv.positive_int }).extend(cv.COMPONENT_SCHEMA)

async def to_code(config):
    server = cg.new_Pvariable(config[CONF_ID])
    await cg.register_component(server, config)
    await fan.register_fan(server, config)

#    var = cg.new_Pvariable(config[CONF_ID])
#    yield cg.register_component(var, config)

#    fan_ = yield fan.create_fan_state(config)
#    cg.add(var.set_fan(fan_))
#    output_ = yield cg.get_variable(config[CONF_OUTPUT])
#    cg.add(var.set_output(output_))

#    if CONF_OSCILLATION_OUTPUT in config:
#        oscillation_output = yield cg.get_variable(config[CONF_OSCILLATION_OUTPUT])
#        cg.add(var.set_oscillating(oscillation_output))

#    if CONF_DIRECTION_OUTPUT in config:
#        direction_output = yield cg.get_variable(config[CONF_DIRECTION_OUTPUT])
#        cg.add(var.set_direction(direction_output))

    cg.add(server.set_fan_id(config[CONF_FAN_DATAPOINT]))