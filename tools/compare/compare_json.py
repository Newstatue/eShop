import json
import os
from deepdiff import DeepDiff

def detect_json_files():
    json_files = [f for f in os.listdir('.') if f.endswith('.json')]
    if len(json_files) < 2:
        raise ValueError("Not enough JSON files to compare. Ensure at least two JSON files are present.")
    return json_files[:2]

def compare_json_files(file1, file2, output_file):
    with open(file1, 'r', encoding='utf-8') as f1, open(file2, 'r', encoding='utf-8') as f2:
        data1 = json.load(f1)
        data2 = json.load(f2)

    # Enhanced DeepDiff configuration
    differences = DeepDiff(
        data1, 
        data2, 
        ignore_order=True, 
        report_repetition=True,
        verbose_level=2  # Ensure detailed differences are captured
    )

    # Write differences to output file
    with open(output_file, 'w', encoding='utf-8') as output:
        output.write(differences.to_json())

    # Print differences summary to console
    print("Differences Summary:")
    print(differences.pretty())

if __name__ == "__main__":
    try:
        file1, file2 = detect_json_files()
        output_file = "differences.json"
        compare_json_files(file1, file2, output_file)
        print(f"Compared {file1} and {file2}. Differences have been written to {output_file}")
    except ValueError as e:
        print(e)