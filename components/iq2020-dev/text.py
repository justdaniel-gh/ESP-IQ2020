import esphome.codegen as cg
import esphome.config_validation as cv
from esphome.components import text
from esphome.const import CONF_ID

from . import ns, IQ2020Component

CONF_IQ2020_ID = "IQ2020Component";
CONF_IQ2020_SERVER = "iq2020_server"
CONF_TEXT_DATAPOINT = "datapoint"

iq2020_text_ns = cg.esphome_ns.namespace('iq2020_text')
IQ2020Text = iq2020_text_ns.class_('IQ2020Text', text.Text, cg.Component)

CONFIG_SCHEMA = text.TEXT_SCHEMA.extend({
    cv.GenerateID(): cv.declare_id(IQ2020Text)
}).extend({ cv.Required(CONF_TEXT_DATAPOINT): cv.positive_int }).extend(cv.COMPONENT_SCHEMA)

async def to_code(config):
    server = cg.new_Pvariable(config[CONF_ID])
    await cg.register_component(server, config)
    await text.register_text(server, config)

#    paren = await cg.get_variable(config[CONF_IQ2020_ID])
#    cg.add(server.set_iq2020_parent(paren))
#   cg.add(server.set_text_id(config[CONF_ID]))
    cg.add(server.set_text_id(config[CONF_TEXT_DATAPOINT]))