import os
import re
import shutil
from distutils.dir_util import copy_tree

def replace_ssi_with_content(target_directory, html_content):
    include_pattern = re.compile(r'<!--#include file="([^"]+)" -->')
    matches = include_pattern.findall(html_content)

    for match in matches:
        print("Match", match)
        include_file_path = os.path.join( target_directory, match )
        with open(include_file_path, 'r', encoding='utf-8') as include_file:
            include_content = include_file.read()
            include_directive = f'<!--#include file="{match}" -->'
            html_content = html_content.replace(include_directive, include_content)

    return html_content

def process_html_file(target_directory, file_path):
    print("Reading", file_path)
    with open(file_path, 'r', encoding='utf-8') as html_file:
        html_content = html_file.read()
        modified_html_content = replace_ssi_with_content(target_directory, html_content)

    with open(file_path, 'w', encoding='utf-8') as modified_file:
        modified_file.write(modified_html_content)

def process_html_files(target_directory):
    # Get a list of all HTML files in the current directory
    html_files = [f for f in os.listdir(target_directory) if f.endswith('.html')]
    
    # Process each HTML file
    for html_file in html_files:
        process_html_file(target_directory, os.path.join(target_directory, html_file))

def setup_target_dir(target_directory):
    # Create the target directory if it doesn't exist
    if not os.path.exists(target_directory):
        print("Creating directory", target_directory)
        os.makedirs(target_directory)
    else:
        print("Deleteting directory contect", target_directory)
        # Delete directory content, except .git folder
        for filename in os.listdir(target_directory):
            if filename == '.git':
                continue
            file_path = os.path.join(target_directory, filename)
            if os.path.isfile(file_path) or os.path.islink(file_path):
                os.unlink(file_path)
            elif os.path.isdir(file_path):
                shutil.rmtree(file_path)

def main():

    source_directory = 'Content'
    target_directory = '..\..\lsigxextensions-documentation'
        
    # Create or clean target directory
    setup_target_dir(target_directory)
    
    # Copy source directory content to target directory
    print(f"Copying content from {source_directory} to {target_directory}")
    copy_tree(source_directory, target_directory)

    # Process SSI includes
    print("Processing SSI includes")
    process_html_files(target_directory)
    
if __name__ == "__main__":
    main()