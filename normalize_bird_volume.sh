#!/bin/bash

# Prompt for the directory containing WAV files
read -p "Enter the directory containing WAV files: " DIRECTORY

# Target volume level in dB (e.g., -1 for -1 dB)
TARGET_VOLUME="0dB"

# Check if ffmpeg is installed
if ! command -v ffmpeg &> /dev/null; then
    echo "ffmpeg could not be found. Please install it first."
    exit 1
fi

# Normalize volume for each WAV file in the directory
shopt -s nullglob  # Enable nullglob to handle no matches
for file in "$DIRECTORY"/*.wav; do
    if [ -f "$file" ]; then
        # Get the base name of the file
        base_name=$(basename "$file")
        # Output file name
        temp_file="$DIRECTORY/normalized_$base_name"
        
        # Normalize the volume and write to the temporary file
        ffmpeg -i "$file" -filter:a "volume=$TARGET_VOLUME" "$temp_file"
        
        # Move the temporary file to overwrite the original file
        mv "$temp_file" "$file"
        
        echo "Normalized $file"
    else
        echo "No WAV files found in the directory."
    fi
done
