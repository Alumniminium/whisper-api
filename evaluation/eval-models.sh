#!/bin/bash

# Get list of models
models=$(curl http://localhost:5045/models | sort)

# Iterate over models and run command
while read -r model; do
  model_name=$(echo "$model" | awk '{print $1}')
  output_file=$(echo "$model_name" | sed 's/\..*/.txt/')

  # Check if output file exists
  if [ -f "$output_file" ]; then
    echo "$output_file already exists. Skipping $model_name..."
    continue
  fi

  echo "Evaluating $model_name -> $output_file..."
  command="curl -sNX 'POST' 'http://localhost:5045/stream?model=$model_name&timestamps=false&singleLine=true&statistics=true' -H 'accept: text/plain' -H 'Content-Type: multipart/form-data' -F 'file=@The TV Screen.mp3;type=audio/mpeg' | tee  $output_file"
  eval "$command"
  cleaned=$(cat $output_file | sed 's/^\s*\[[^]]*\]\s*//; s/\s*\[[^]]*\]\s*$//')
  echo $cleaned > $output_file
done <<< "$models"
