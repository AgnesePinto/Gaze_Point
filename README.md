# Eye Tracking as a Complementary Input Channel for Form Filling
**Bachelor’s Thesis in Electronic and Computer Engineering – University of Pavia**

Developed at the **Computer Vision and Multimedia Laboratory (CVMLab)**  
**Advisor:** Marco Porta  
**Co-Advisor:** Piercarlo Dondi

Human-computer interaction framework based on eye-tracking for Windows applications (WPF). Thanks to Gazepoint technology, the system transforms gaze into an active pointing device, implementing filtering and navigation-assistance logic to make forms filling more efficient.

---

## Table of Contents
* [Description](#description)
* [Key Features](#2-key-features)
* [Hardware & Software Requirements](#3-hardware--software-requirements)
* [Installation & Setup](#4-installation--setup)
* [Usage](#5-usage)
* [Architecture & Project Structure](#6-architecture--project-structure)
* [Technologies](#7-technologies)
* [Contributors & Acknowledgments](#8-contributors--acknowledgments)
* [License](#9-license)

---

## Description
This project explores the use of eye-tracking as a complementary input channel for human-computer interaction (HCI), with a specific focus on form filling.
The framework integrates Gazepoint devices to transform eye movement into input actions, enabling a multimodal interaction that complements the use of mouse and keyboard.

### The System and Interface
Unlike traditional eye-pointing systems, which require dedicated interfaces with very large and widely spaced UI elements, this software is designed to operate on forms with standard layouts, typical of major data-entry applications.  
The system supports interaction with the main graphical controls:
* **Text input:** TextBox
* **Single / multiple choice:** RadioButton, CheckBox
* **Dynamic selection controls:** ComboBox
* **Action controls:** Button
* **Navigation elements:** management of lists and application windows

Thus, eye input does not replace traditional methods but complements them, enabling a multimodal interaction.

### Functioning and Control Logic                                                                                                                     
The core of the system consists of transforming the raw signal from the Gazepoint tracker into intentional and stable commands.  
This process is achieved through three main components:
* **Signal Processing**                                        
  * Adaptive smoothing to reduce the natural gaze jitter
  * Validation and blink recovery to handle temporary tracking loss
* **Real-Time Data Analysis**
  * Analysis of the ocular signal in real time
  * Distinction between visual exploration and command intention through dwell-time algorithms
* **Active Navigation**
  * The system does not merely replicate a passive pointer.
  * Through movement detection and target-locking modules, the framework analyzes micro-eye movements to assist the user in selecting small elements and in smoothly transitioning between adjacent fields.

### Objectives and Application Domains
The main objective is to improve the efficiency of data-entry operations in continuous-work scenarios.  
The framework is intended for professional contexts where operators handle large volumes of forms or management interfaces.  
Integrating gaze as an input channel reduces physical and cognitive load, supporting a workflow based on the combination of gaze interaction and mouse/keyboard input.

---

## 2. Key Features

**Signal Processing & Stability**
* **Adaptive Low-Pass Filtering**                                                                         
  Implementation of a first-order low-pass filter with a dynamic coefficient designed to stabilize the eye signal while maintaining high responsiveness.
  The algorithm reduces smoothing during rapid eye movements to ensure pointing speed and increases it during fixations to eliminate the natural jitter of the signal.
* **Validation & Blink Recovery**                                                                               
  Filtering system that monitors the validity of the received gaze data.
  Introduces a configurable blanking period that freezes the cursor position during blinks and ignores unstable samples immediately after the eye reopens, ensuring visual continuity and preventing sudden cursor jumps.
* **Custom Gaze Cursor**                                                                              
  High-priority visual cursor designed for gaze-based interaction.
  Using the Win32 API (`SetWindowPos`), the system overcomes the rendering stack limitations of WPF and ensures that the cursor remains topmost, even above Popup controls or system windows.

**Intelligent Interaction & Navigation**
* **Dwell-Time Management**                                                                                                                                        
  Control activation system based on the duration of gaze fixation.
  The module monitors the time the gaze remains on an interactive element and triggers focus or click events only when a configurable temporal threshold is exceeded, effectively distinguishing visual exploration from intentional interaction.
* **Saccade Detection**                                                                                    
  Analyzes the magnitude of rapid movements between consecutive fixation points.
  The module detects ocular saccades and temporarily suspends the dwell logic to prevent accidental activations during rapid interface scanning.
* **Movement Analysis & Directional Navigation**                                                                                             
  Algorithm that analyzes micro-eye movements collected during a target-lock time window.
  Movements are classified as small steps or large jumps, enabling assisted navigation between nearby and adjacent input fields.
* **Dynamic Visual Tree Scanning**                                                                                                        
  The TargetProvider module performs a recursive scan of the WPF Visual Tree to dynamically map the coordinates of interactive interface elements.
  The system supports runtime-generated controls and components contained in separate Popups, ensuring robust interaction even in complex WPF interfaces.

---

## 3. Hardware & Software Requirements  
The system was developed and tested to operate in a Windows environment with Gazepoint eye-tracking hardware integration.
Below are the requirements for running and developing the project:

**Hardware**
* **Eye Tracker:** Gazepoint GP3 HD                                                              
  Compatible with later models supporting the OpenGaze protocol.
* **Display:** Standard resolution monitor                                                                    
  The system manages DPI scaling via WPF’s logical coordinate system. 

**Operating System**
* Windows 10
* Windows 11

**Software & Driver**
* **Gazepoint Control Center (OpenGaze)**                                                                                         
  The control server must be active and calibrated to allow eye-tracking data streaming.
* **Data Streaming**                                                                                                        
  TCP/IP protocol on the default port 4242.

**Development & Build Environment**
* **IDE:** Visual Studio 2022 (or versions compatible with .NET Desktop development).
* **Framework:** .NET Framework 4.7.2.
* **External Libraries (NuGet):**
  * `Microsoft.Extensions.Configuration`                                                                                       
    Used for managing and loading JSON configuration files.
  * `System.Runtime.InteropServices`                                                                                             
    Used for integration with Win32 APIs.

---

## 4. Installation & Setup                                                            
To configure the development environment and run the project, follow these steps:

**Step 1: Clone the Repository**  
Clone the repository via Git:
```bash
git clone https://github.com/AgnesePinto/Gaze_Point.git
```

**Step 2: Restore Dependencies**                                                                                         
Open the solution file (`.sln`) with Visual Studio 2022.                                                                  
The system should automatically restore missing NuGet packages. If not, execute:                                                            
`Tools > NuGet Package Manager > Restore NuGet Packages`.

**Step 3: Configure JSON Files**                                                  
Ensure that the files inside the `AppSettings/` folder are correctly configured.
> **Important Note:** in Visual Studio properties, set “Copy to Output Directory” to “Copy if newer” for both files.

* `Connection.json`: verify that the IP address is `127.0.0.1` and the port is `4242`.
* `DataSettings.json`: verify the filtering parameters.  
  > **Attention:** use a dot (`.`) as the decimal separator (e.g., `0.10`) to ensure compatibility with the InvariantCulture parser.

---

## 5. Usage
To correctly use the gaze interaction system, follow the operational sequence below.  
This procedure ensures that the data stream is active and calibrated before starting the WPF logic.

**1. Launch Gazepoint Control Center**  
Run the Gazepoint Control software provided with the tracker.  
This software acts as a server for the framework, managing low-level communication with the GP3 HD sensor.

**2. Calibrate the System**  
Within the Control Center, start the calibration procedure (5 or 9 points):
* Ensure that the user is positioned at the correct distance from the sensor.
* Verify that both eyes are properly tracked in the camera preview window.
* Ensure tracking quality is stable before proceeding.
* Accurate calibration is essential to guarantee interaction precision.

**3. Verify Streaming (Port 4242)**  
Before launching the application, ensure that the Gazepoint server is transmitting data.  
The framework will automatically establish a connection with:
* **IP:** `127.0.0.1`
* **Port:** `4242`
After starting the application, you can check the connection status in the console logs, which will confirm a successful handshake:
`Successfully connected to the server`

**4. Launch the Application**  
Run the project from Visual Studio or execute the compiled application.  
The system automatically manages the display of the gaze cursor according to the build configuration:                                          
* **Debug Mode:**
   * The visual cursor (red circle) is visible.
   * Allows real-time monitoring of smoothing filter behavior and gaze tracking accuracy.
   * Useful for debugging, development, and system validation phases.
* **Release Mode:**
   * The cursor is hidden.
   * Interaction occurs directly through fixation (dwell time) on interface elements.
   * This mode provides a more natural and less intrusive experience for the end user.

---

## 6. Architecture & Project Structure  
The framework is developed following the MVVM (Model–View–ViewModel) architectural pattern, ensuring a clear separation between data acquisition logic, signal processing, and the user interface.

**Project Directory Tree**
```text
Gaze_Point/
│
├── AppSettings/           # JSON configurations (connection and filter parameters)
├── Connection/            # Management of TCP/IP communication with the tracker (OpenGaze)
├── GPModel/
│   ├── GPCursor/          # Models for gaze pointer representation
│   ├── GPInteraction/     # Interaction logic (dwell, targeting, locking)
│   └── GPRecord/          # Data parsing and stabilization filters
├── GPViewModel/           # Application logic and command bindings
│   └── Handlers/          # Interaction strategies for UI controls
├── Services/              # Core services and cursor management
├── Themes/                # Graphical resources and XAML styles
└── View/                  # WPF windows and components
```

**Main Components**

* **Connection (GPClient)**                                                                                                                      
  Manages the TCP/IP connection with the tracker through the OpenGaze protocol and reconstructs the XML packets received from the data stream.

* **GPRecord (Parser & Filters)**                                                                                                                                 
  Responsible for parsing the raw data from the tracker and applying smoothing filters to reduce jitter and handle conditions such as blinking or temporary signal loss.

* **GPInteraction**                                                                                                                                            
  Implements gaze interaction logic, including:
  * Identification of interactive elements within the UI
  * Management of dwell-based interaction
  * Assisted navigation mechanisms between controls

* **Handlers**                                                                                                                     
  Define specific strategies for interaction with different WPF controls (e.g., Button, TextBox, ComboBox, etc.).

* **Services (GPService)**                                                                                                                                      
  Acts as the main orchestrator of the system, coordinating:
  * Data flow from the tracker
  * Interaction logic
  * Updates to the gaze cursor

---

## 7. Technologies
The framework was developed using the following technologies and libraries:
* **Programming Language:** C# (C Sharp)
* **UI Framework:** WPF (Windows Presentation Foundation)
* **Core Framework:** .NET Framework 4.7.2
* **Communication Protocol:** OpenGaze (XML over TCP/IP)
* **External Libraries (NuGet Packages):**
  * `Microsoft.Extensions.Configuration`: dynamic loading and management of JSON configuration files.
  * `Microsoft.Extensions.Configuration.Json`: specific support for JSON format parsing.
* **OS Interoperability:** Win32 API (P/Invoke) for advanced control of positioning and Z-order priority of the gaze cursor.
* **Development Environment:** Visual Studio 2022.

---

## 8. Contributors & Acknowledgments
This project was developed as a Bachelor’s Thesis in Electronic and Computer Engineering at the University of Pavia.

* **Author:** Agnese Pinto
* **Research Laboratory:** Computer Vision and Multimedia Laboratory (CVMLab), University of Pavia
* **Academic Supervision:**
  * Advisor: Marco Porta
  * Co-Advisor: Piercarlo Dondi

The CVMLab laboratory provided the Gazepoint eye-tracking hardware and the experimental environment used for the validation of the system.

---

## 9. License
This project is distributed under the MIT License.  
Refer to the `LICENSE` file included in the repository for further details.
