#include "esphome/core/log.h"
#include "iq2020_fan.h"
#include "iq2020.h"

extern IQ2020Component* g_iq2020_main;
extern esphome::iq2020_fan::IQ2020Fan* g_iq2020_fan[FANCOUNT];

namespace esphome {
namespace iq2020_fan {

	static const char *TAG = "iq2020_fan.fan";

	void IQ2020Fan::setup() {
		if (fan_id < FANCOUNT) { g_iq2020_fan[fan_id] = this; }
		ESP_LOGD(TAG, "Fan:%d Setup", fan_id);
	}

	void IQ2020Fan::dump_config() {
		ESP_LOGCONFIG(TAG, "IQ2020 fan");
	}

	esphome::fan::FanTraits IQ2020Fan::get_traits() {
		auto traits = fan::FanTraits(false, true, false, 2);
		//FanTraits(bool oscillation, bool speed, bool direction, int speed_count)
		//traits.set_direction(false);
		//traits.set_oscillation(false);
		//traits.set_speed(true);
		//traits.speed_count(2);
		return traits;
	};

	void IQ2020Fan::control(const esphome::fan::FanCall &call) {
		ESP_LOGCONFIG(TAG, "IQ2020 fan control");
	};

}  // namespace binary
}  // namespace iq2020_fan
