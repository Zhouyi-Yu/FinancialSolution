# Plan B: Professional Demo Video Strategy 🎥

If you cannot complete the AWS deployment before the interview, this video acts as your **Plan B** to prove your skills. 

## 🛠️ Recording Setup
- **Tool**: OBS Studio or Loom (recommended for browser + webcam).
- **Duration**: Target **3 to 5 minutes**.
- **Mode**: "Full Stack Developer walkthrough".

## 📽️ The Storyboard (Minute by Minute)

### Minute 1: The High-Level Architecture
- **Visual**: Show the `README.md` diagram and the `deploy-aws.yml`.
- **Script**: "Hi, I'm Zhouyi. Before we dive into the app, I want to show the infrastructure. This project is built for **AWS high-availability** using App Runner for the .NET API and RDS for the database. I've already integrated the **GitHub Actions** pipeline you see here to automate the container build to Amazon ECR."

### Minute 2: The Logic (Backend)
- **Visual**: Show `backend/FinanceApi/Program.cs` and a Controller.
- **Script**: "The backend is C# .NET 8. I've focused heavily on **Application Security**—implementing JWT authentication and strict CORS policies. You can see here how I use the Repository pattern to keep the data layer decoupled from the API logic."

### Minute 3: The Interaction (Frontend)
- **Visual**: Toggle the Vue.js dashboard (running locally).
- **Script**: "The frontend is Vue 3 with the Composition API. I used **Pinia** for state management to ensure that financial data stays synchronized across components without redundant network hits. Notice the sub-100ms response time—that's partially due to the Redis caching layer I've architected into the system."

### Minute 4: The BI Proof (The "Wow" Factor)
- **Visual**: Show the `power_bi/DESIGN.md` and then run `python3 power_bi_model.py`.
- **Script**: "Unique to this project is the BI integration. Since I'm on Mac but target Power BI environments, I've designed the **Star Schema** here and written a Python simulation to verify that my **DAX measures** like MoM growth calculate correctly against my Postgres data."

### Minute 5: Closing
- **Visual**: Show your GitHub commit history.
- **Script**: "I treat my projects like production systems. I follow a clear branching strategy, keep a detailed `CHANGELOG.md`, and focus on observability through structured logging."

## 💡 Interview Tip:
If they ask why it's a video instead of a live URL:
*"I have the full deployment scripts ready, but for the interview, I wanted to provide a 'Guided Tour' of the architecture to save time and ensure we can focus on the specific logic and security decisions I made."*
