# 🤖 Multimodal Zero-Shot Activity Recognition for Process Mining of Robotic Systems

> **Understanding what a robot has done by observing video + audio, without training a model.**  
> ✅ Activity recognition → ✅ Event log → ✅ Process mining (DFG)

---

## 📌 Project Objective

This project implements a pipeline to **observe a robot**, **recognize the activities it performs**, and **reconstruct the process of its actions**.

In short:

1. The robot is monitored via **video + audio**  
2. A multimodal foundation model recognises activities in a **zero-shot** manner (no task-specific training)  
3. The activities are converted into an **event log**  
4. We apply **process mining** to derive a process model (DFG)

---

## 🧠 Key Idea

This project leverages **multimodal foundation models** (e.g., LLava / Phi-Vision / Llama-Vision) to perform **zero-shot activity recognition** in a robotic setting.

### Why this matters

- 🚫 No bespoke training required  
- ⚙️ Easily portable to different robots/scenarios by *just changing the prompt*  
- 🎯 Ideal for industrial robotic scenarios or where custom training is infeasible

---

## 🎥 Dataset / Scenario

Following the referenced paper, the robot manipulates objects (e.g., Baxter UR5 dataset) and performs activities such as:

| Activity | Description |
|---|---|
| grasp | grasping an object |
| pick | lifting it |
| hold | holding it |
| shake | manipulating it |
| lower | lowering it |
| drop | releasing an object (audio helps detect this!) |

We combine **video + audio** modalities for higher reliability:

🖼️ Video = good for most actions  
🔊 Audio = especially helpful to detect *drop* events

---

## 📂 Architecture

| Step | Description | Technology |
|---|---|---|
| 1 | Extract frames from video + audio chunks | C# + FFmpeg wrapper |
| 2 | Multimodal prompt for zero-shot activity recognition | Semantic Kernel + Ollama |
| 3 | Recognize activities | vision model (LLava / Phi-Vision / Moondream / Llama-Vision) |
| 4 | Audio + video fusion logic | C# |
| 5 | Generate event log in CSV (`<caseId, timestamp, activity>`) | C# |
| 6 | Perform process mining → derive DFG | pm4net (.NET lib) or export for Disco/ProM |

---

## 🏗 Pipeline Flow

Video + Audio
↓
Frame extraction (1 fps) + audio chunks
↓
Multimodal prompt → zero-shot model
↓
Predicted activity labels
↓
Audio check (especially for “drop”)
↓
Event Log (CSV)
↓
Process Mining → DFG

---

## 🧠 Prompt Engineering

The prompt describes the set of activities and provides instructions on how to recognize them.  
This is the **core** of the project.

> ✅ Zero-shot strategy = no training, prompt + foundation model only

---

## 🚀 How to Replicate

### Requirements
.NET 8+
Semantic Kernel
Ollama
FFmpeg

### Recommended models on Ollama

- `llava`
- `phi3-vision`
- `moondream`
- (if available) `llama-vision`

---

## 📎 Reference Abstract

For the original abstract and detailed motivation of this research project, see:  
[Abstract – “Multimodal Zero-Shot Activity Recognition for Process Mining of Robotic Systems”](https://www.researchgate.net/publication/395101773_Multimodal_Zero-Shot_Activity_Recognition_for_Process_Mining_of_Robotic_Systems)  

---

## 📦 Roadmap

- [ ] Video/audio extraction  
- [ ] Semantic Kernel prompt for recognition  
- [ ] Activity recognition pipeline  
- [ ] Audio-video fusion (detect “drop”)  
- [ ] Generate event log  
- [ ] Support pm4net + export to Disco/ProM  
- [ ] Visualize DFG  

---

Give this repository a ⭐ on GitHub and share it!

```bash
https://github.com/sofiacuccu00/MultimodalActivityRecognition_RoboticSystems.git
```
