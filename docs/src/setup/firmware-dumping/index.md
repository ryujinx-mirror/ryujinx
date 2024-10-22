Thanks to Candy for providing the guide details & images. Loosely based off of [NH Switch Guide](https://nh-server.github.io/switch-guide/extras/updating/#updating-emummc-by-dumping-an-updated-firmware-from-your-sysmmc).

What you will need:
- The latest release of [TegraExplorer](https://github.com/suchmememanyskill/TegraExplorer/releases) (the `.bin` file)
---

1. Make sure your sysMMC is up to date. If your sysMMC is not up to date, update it through System Settings.

2. Boot your Switch into RCM.

3. Using a Payload Injector, such as TegraRcmGUI or fusee-interfacee-tk, inject the TegraExplorer.bin Payload (like you would with Hekate).

![Screenshot 2022-12-03 at 10 45 13 PM](assets/205474096-25049c51-9659-4122-a4f1-c2fc2eb00a4d.png)

4. Using the joystick and the A buttons, select `FirmwareDump.te`.

![Screenshot_0a75c7c9](assets/205474107-a103b5e8-6b31-42f2-ba34-a798172931cb.png)

5. Select `Dump sysmmc`.

![Screenshot_0ad4061c](assets/205474109-8c5fb59c-99c8-42a3-afbb-4da61865335b.png)

6. Wait about 1-2 minutes for the tool to dump your firmware.

![Screenshot_0d9ca2b9](assets/205474110-14016849-477b-486b-989f-6b713ec9cc74.png)

7. When the tool finishes, press any button.

8. Select `Power off`.

![Screenshot_2dec46de](assets/205474112-5e6bcb81-a46e-40ff-85df-4edea0f0a66a.png)

9. Remove the SD card from your Switch, and insert it into your PC.

10. A folder containing your firmware should now exist at `/tegraexplorer/Firmware/<version-number>`

![Screenshot 2022-12-03 at 11 09 38 PM](assets/205474104-0aba8839-aee8-4ad9-bdbe-ac07e390d73a.png)

