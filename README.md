# Garmin_To_Fitbit_Steps_Sync_Web


## Development Roadmap
* Need to have confidence in this app to input data into FitBit Correctly so that we don't ever have to login on the fitbit homepage
* \[Useability\] Only Show the Add Activity and other buttons when we are already connected to FitBit.
* Need to implement Azure keyvault - Currently Azure WebApp cannot access app secrets

## Possible Features
* Make it visible to user if activities have been input for the previous days, so we don't forget or create a duplicate entry
* Calcuate Our Average for the Month (using our best 21 days)  - Could we calculate this as [Need to Average x steps over the next x days] 157,500 steps in a month

## Recently Completed Features
* Give us our average steps for the last 7 days here in the APP

### 08/12/2020
adding a drop-down to select the day for the activity. We usually input our steps for the previous day, so this will be the default. We will also provide an option to put in
the day before yesterday's step count.

### 09/13/2020
* Created README.md

### 09/18/2020
* Experimenting with activity time series. It would be helpful to get a snapshop of the last 7 days of steps
* Hide the Connect to FitBit button if we are already connected and don't show the functionality buttons if we are connected. Once we are connected, show the buttons!


