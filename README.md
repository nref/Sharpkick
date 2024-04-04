# Sharpkick

Automation for Gigabyte monitors

## Why?

OSD Sidekick only talks to one monitor, and it requires you to click buttons on its user interface. Sharpkick talks to all detected Gigabyte monitors and adds automations, like performing an action they are added or removed.

In particular, the `autokvm` command listens for Gigabyte monitors to appear as USB HID devices and then toggles their KVM setting. See the Background section below why I made this.

## Examples

```powershell
> .\sharpkick autokvm ` 
  --toggle 0 `
  --debounce-interval 00:00:05 `
  --poll-interval 00:00:01 `
  --device-paths `
    "\\?\hid#vid_0bda&pid_1100#d&2a64e1ff&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}" `
    "\\?\hid#vid_0bda&pid_1100#d&3b408ceb&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"

[13:33:21 INF] Ready to autoswitch monitors.
[13:33:21 INF] Listening for:
  \\?\hid#vid_0bda&pid_1100#d&2a64e1ff&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
  \\?\hid#vid_0bda&pid_1100#d&3b408ceb&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
```

```powershell
> .\sharpkick watch --poll-interval 00:00:01
[13:33:59 INF] Listening for monitors
[13:34:32 INF] Added: \\?\hid#vid_0bda&pid_1100#d&2a64e1ff&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
[13:34:32 INF] Added: \\?\hid#vid_0bda&pid_1100#d&3b408ceb&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
```

## Background

I have a Sabrent [KVM switch](https://sabrent.com/products/sb-tb4k), a desktop computer, a laptop computer, and two Gigabyte M32UC monitors.

The Sabrent outputs to the USB-C input on each monitor. The desktop outputs to the DisplayPort input on each monitor. Meanwhile both the laptop and desktop are connected to the USB-C inputs on the KVM switch. For the desktop, only USB devices over USB-C; there is no pass-through of the graphics card. For the laptop, both USB devices and the graphics card are connected to the Sabrent over USB-C.

Topology:

```
Laptop  ==(USB-C)==>       KVM
Desktop ==(USB-C)==>       KVM
KVM     ==(USB-C)==>       M32UC #1
KVM     ==(USB-C)==>       M32UC #2
Desktop ==(DisplayPort)==> M32UC #1
Desktop ==(DisplayPort)==> M32UC #2
```

When I press the KVM button to toggle computers, both monitors' USB devices toggle; from the perspective of the old USB host, the USB devices are unplugged, and on the new USB host, the USB devices are plugged in. 

However, the story for the display is more complicated. Since the desktop is connected by DisplayPort, its input signal does not stop when I press the Sabrent button, so the monitors do not switch over to the laptop. The monitors do detect the new USB-C signal and flash the OSD option to press the KVM button on the back of the monitors. However, I do not want to reach around behind the monitors as it is awkward. Instead, I want to switch devices simply with a press of the Sabrent button. Moreover, the new USB-C signal is not ready when advertised; if I press the KVM button behind each monitor, the each monitor reverts back to DisplayPort. The KVM pop-up times out before USB-C is ready. Therefore on each monitor I have to manually bring up the OSD menu and switch inputs.

## Configuration

### Poll Interval

You can configure how often the app checks for monitors. Try `--poll-interval 00:00:01`.

### Debouncing

You can configure how long the app waits after detecting the monitors before telling them to switch. Try `debounce-interval 00:00:05`.

This is useful since switching too quickly causes the monitor to revert back to DisplayPort since USB-C is not yet sending a display signal.

### Device Paths

By default, the app reports all Gigabyte M32U / M32UC monitors. However, you can configure the app to only listen to specific device paths. Try `--device-paths <path1> <path2> ... <pathN>`

You can discover your device paths with the `watch` command.

This supports the following use case:

After my Sabrent KVM switches from the desktop to the laptop, the monitors connect to the laptop as HID devices over USB-A. Then `sharpkick` sends a USB message to each monitor to toggle their KVMs from DisplayPort to USB-C. Then each monitor unplugs its original HID device from USB-A and replugs as *different* HID devices over USB-C. The app by default would send the KVM toggle message also those new devices. In my case this causes the monitors to switch back to DisplayPort. 

## Credit

Gigabyte OSD Sidekick is a Windows-only app for controlling Gigabyte monitors. Others have worked to bring its features to Linux and macOS, e.g. https://github.com/kelvie/gbmonctl which sends messages to the monitors over USB.
