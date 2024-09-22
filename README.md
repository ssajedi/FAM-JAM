<img src="https://github.com/ssajedi/FAM-JAM/blob/main/assets/Logo.svg" width="300">

# FAM JAM
**Intelligent Spec Management for Revit**

FAM JAM is a Revit Plugin developed during the 2024 [AECTech Hackathon](https://www.aectech.us/la-event). We created a solution aimed at simplifying specification sheet management for Revit, targeting architectural workflows. Fam Jam allows users to seamlessly integrate Revit families with specification sheets, addressing a common bottleneck by turning families into specs, making the process more intuitive and efficient. 

# Prerequisites
In order to use FAM JAM, you will require:
* Revit
* [OpenAI API key](https://platform.openai.com/)
  
# Getting Started
To use this repository please follow these steps:
### Part I
* Clone the repository using <git>
* Generate an OpenAI API key
* Create `secret.txt` and add the API Key.
* Place `secret.txt` inside the main path of your repository . 
### Part II (Revit):
* Locate the repository on your computer.
* Extract [plugin-files.zip](https://github.com/ssajedi/FAM-JAM/blob/main/plugin-files.zip) which is a file in the main root.
⚠️ Please make sure to complete the next two steps in the given sequence. 
* Copy `FAM JAM` directory after extraction to `C:\ProgramData\Autodesk\Revit\Addins\{YOUR_REVIT_VERSION}`.
* Copy `FamJam.addin` to `C:\ProgramData\Autodesk\Revit\Addins\{YOUR_REVIT_VERSION}`
In the provided path, replace `YOUR_REVIT_VERSION` with proper values (e.g., 2023).

# Demo 
Please refer to the following video for a demonstration of how FAM JAM.

# Future work
For future work, we are focusing on several key areas of development to enhance system performance and usability even further:

* Further expansion of the .rfa family database
* Enhanced prompting mechanisms: The API usage costs can potentially be reduced by optimized the image resolutions in the API requests. 
* AI-driven insights for asset selection: We plan to adopt an AI semantic search feature in the workflow, allowing for even smarter asset selection and flitering.
* Improved credential management: Upcoming improvements will include more sophisticated security protocols and user-friendly interfaces for managing credentials

# Team

| ![Nehansh Saxena](https://github.com/ssajedi/FAM-JAM/blob/main/assets/nehansh.jpg) | ![Frank(Xu) Li](https://github.com/ssajedi/FAM-JAM/blob/main/assets/Frank.jpg) | ![Seyedomid Sajedi](https://github.com/ssajedi/FAM-JAM/blob/main/assets/Omid.jpg) |
|:--:|:--:|:--:|
| [**Nehansh Saxena**](https://www.linkedin.com/in/nehansh-saxena-leed-ga-assoc-aia-137982127/) | [**Frank(Xu) Li**](https://www.linkedin.com/in/frankeng/) | [**Seyedomid Sajedi**](https://www.linkedin.com/in/seyedomid-sajedi-263b703a/) |
| Architectural Designer<br>S/L/A/M Collaborative<br>Los Angeles, CA | Senior Project Engineer<br>Saiful Bouquet<br>Irvine, CA | Associate AI/ML Engineer<br>Thornton Tomasetti<br>NYC, NY |
