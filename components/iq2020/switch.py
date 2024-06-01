import esphome.codegen as cg
import esphome.config_validation as cv
from esphome.components import switch
from esphome.const import CONF_ID

from . import ns, IQ2020Component

CONF_IQ2020_ID = "IQ2020Component";
CONF_IQ2020_SERVER = "iq2020_server"
CONF_SWITCH_LIGHTS = "iq2020_switch"

#iq2020_switch_ns = cg.esphome_ns.namespace('iq2020_switch')
#EmptySwitch = iq2020_switch_ns.class_('IQ2020Switch', switch.Switch, cg.Component)

#CONFIG_SCHEMA = switch.SWITCH_SCHEMA.extend({
#    cv.GenerateID(): cv.declare_id(EmptySwitch)
#}).extend(cv.COMPONENT_SCHEMA)

IQ2020Switch = ens.class_(
    "IQ2020Switch", switch.Switch, cg.Component
)

CONFIG_SCHEMA = (
    switch.switch_schema(IQ2020Switch)
    .extend(
        {
            cv.Required(CONF_SWITCH_DATAPOINT): cv.string,
        }
    )
    .extend(cv.COMPONENT_SCHEMA)
)

async def to_code(config):
    server = cg.new_Pvariable(config[CONF_ID])
    await cg.register_component(server, config)
    await switch.register_switch(server, config)

#    paren = await cg.get_variable(config[CONF_IQ2020_ID])
#    cg.add(server.set_iq2020_parent(paren))
#   cg.add(server.set_switch_id(config[CONF_ID]))
#    cg.add(server.set_switch_id(config["name"]))
    cg.add(server.set_switch_id(config[CONF_SWITCH_DATAPOINT]))

