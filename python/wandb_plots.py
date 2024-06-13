import pandas as pd
import matplotlib.pyplot as plt


# Check if log scaling flag is True
log_scale_flag = False
# Read the data from the CSV file
data = pd.read_csv("C:/Users/peipei/Documents/U3Paper/wandb/return.csv", header=None, skiprows=1)

# Convert data to numeric
data = data.apply(pd.to_numeric)

#data.iloc[:, 1] = data.iloc[:, 1].clip(lower=0, upper=1)

# Drop any rows with NaN values
data.dropna(inplace=True)

# Apply exponential smoothing to the data
#data_smoothed = data.ewm(alpha=0.1).mean()

# Apply sliding window mean to the data
window_size = 20  # Adjust the window size as needed
data_smoothed = data.rolling(window=window_size).mean()

# Plotting
plt.figure(figsize=(10, 6))
plt.plot(data_smoothed.iloc[:, 0], data_smoothed.iloc[:, 1], label="global_step (smoothed)")

# Add title and labels
plt.title("Global Steps with Exponential Smoothing")
plt.xlabel("Step")
plt.ylabel("Value")

# Apply log scaling if flag is True
if log_scale_flag:
    plt.yscale('log')

# Add legend
plt.legend()

# Save plot as PDF
plt.savefig("return.pdf")

# Show plot
plt.show()
