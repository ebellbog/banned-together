#!/usr/bin/env python3
"""
Project Gutenberg Downloader and Unity Text Formatter
A utility for downloading public domain texts and preparing them for Unity TextMeshPro
"""

import os
import sys
import re
import requests
import urllib.parse
from pathlib import Path
from typing import List, Dict, Optional


class GutenbergDownloader:
    def __init__(self, base_dir: str = "."):
        self.base_dir = Path(base_dir)
        self.raw_dir = self.base_dir / "books" / "raw"
        self.formatted_dir = self.base_dir / "books" / "formatted"
        self.api_base = "https://gutendex.com"
        
        # Create directories if they don't exist
        self.raw_dir.mkdir(parents=True, exist_ok=True)
        self.formatted_dir.mkdir(parents=True, exist_ok=True)
    
    def search_books(self, title: str) -> List[Dict]:
        """Search for books by title using Gutendex API"""
        try:
            # URL encode the search query
            encoded_title = urllib.parse.quote(title)
            url = f"{self.api_base}/books?search={encoded_title}"
            
            print(f"Searching for '{title}'...")
            response = requests.get(url, timeout=45)
            response.raise_for_status()
            
            data = response.json()
            return data.get('results', [])
            
        except requests.RequestException as e:
            print(f"Error searching for books: {e}")
            return []
    
    def display_search_results(self, books: List[Dict]) -> Optional[Dict]:
        """Display search results and let user choose"""
        if not books:
            print("No books found matching that title.")
            return None
        
        print(f"\nFound {len(books)} matching books:")
        print("-" * 60)
        
        for i, book in enumerate(books[:10], 1):  # Show max 10 results
            title = book.get('title', 'Unknown Title')
            authors = book.get('authors', [])
            author_names = [author.get('name', 'Unknown') for author in authors]
            author_str = ', '.join(author_names) if author_names else 'Unknown Author'
            downloads = book.get('download_count', 0)
            
            print(f"{i:2d}. {title}")
            print(f"    Author(s): {author_str}")
            print(f"    Downloads: {downloads:,}")
            print()
        
        if len(books) > 10:
            print(f"... and {len(books) - 10} more results")
        
        while True:
            try:
                choice = input(f"Select a book (1-{min(len(books), 10)}) or 0 to search again: ").strip()
                
                if choice == '0':
                    return None
                
                choice_num = int(choice)
                if 1 <= choice_num <= min(len(books), 10):
                    return books[choice_num - 1]
                else:
                    print("Invalid selection. Please try again.")
                    
            except ValueError:
                print("Please enter a number.")
            except KeyboardInterrupt:
                print("\nOperation cancelled.")
                return None
    
    def get_text_download_url(self, book: Dict) -> Optional[str]:
        """Extract the plain text download URL from book data"""
        formats = book.get('formats', {})
        
        # Try different text format MIME types in order of preference
        text_types = [
            'text/plain; charset=utf-8',
            'text/plain; charset=us-ascii',
            'text/plain'
        ]
        
        for mime_type in text_types:
            if mime_type in formats:
                return formats[mime_type]
        
        # If no exact match, look for any text/plain format
        for mime_type, url in formats.items():
            if mime_type.startswith('text/plain'):
                return url
        
        return None
    
    def download_book(self, book: Dict) -> Optional[Path]:
        """Download the book's text file"""
        title = book.get('title', 'Unknown Title')
        book_id = book.get('id')
        
        # Get download URL
        download_url = self.get_text_download_url(book)
        if not download_url:
            print("No plain text version available for this book.")
            return None
        
        # Create safe filename
        safe_title = "".join(c for c in title if c.isalnum() or c in (' ', '-', '_')).rstrip()
        safe_title = safe_title[:100]  # Limit length
        filename = f"{book_id}_{safe_title}.txt"
        filepath = self.raw_dir / filename
        
        try:
            print(f"Downloading '{title}'...")
            response = requests.get(download_url, timeout=30)
            response.raise_for_status()
            
            # Write the file
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(response.text)
            
            print(f"✓ Downloaded to: {filepath}")
            return filepath
            
        except requests.RequestException as e:
            print(f"Error downloading book: {e}")
            return None
        except UnicodeDecodeError:
            print("Error: Could not decode text. The file might be in an unsupported format.")
            return None
    
    def format_for_unity(self, input_file: Path) -> Path:
        """
        Format text for Unity TextMeshPro
        Removes boilerplate, fixes line wrapping, handles special characters
        """
        output_file = self.formatted_dir / input_file.name
        
        try:
            print(f"\n📖 Processing '{input_file.name}'...")
            
            with open(input_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            print("  ✓ File loaded successfully")
            
            # Step 1: Remove Project Gutenberg boilerplate
            formatted_content = self._remove_gutenberg_boilerplate(content)
            
            # Step 2: Remove table of contents
            formatted_content = self._remove_table_of_contents(formatted_content)
            
            # Step 3: Fix line wrapping issues
            formatted_content = self._fix_line_wrapping(formatted_content)

            # Step 4: Fix font styling (bold, italic)
            formatted_content = self._fixing_font_styling(formatted_content)

            # Step 5: Remove bracketed asides/notes
            formatted_content = self._remove_bracketed_asides(formatted_content)
            
            # Step 6: Handle special characters
            formatted_content = self._handle_special_characters(formatted_content)
            
            # Step 7: Final cleanup
            formatted_content = self._final_cleanup(formatted_content)
            
            with open(output_file, 'w', encoding='utf-8') as f:
                f.write(formatted_content)
            
            print(f"  ✅ Formatted file saved to: {output_file}")
            return output_file
            
        except Exception as e:
            print(f"  ❌ Error formatting file: {e}")
            return input_file
    
    def _remove_gutenberg_boilerplate(self, content: str) -> str:
        """Remove Project Gutenberg headers and footers"""
        print("  📝 Removing Project Gutenberg boilerplate...")
        
        # Find the start marker (various patterns)
        start_patterns = [
            r'\*\*\* START OF TH[EI]S? PROJECT GUTENBERG EBOOK[^*]*\*\*\*',
            r'\*\*\*START OF TH[EI]S? PROJECT GUTENBERG EBOOK[^*]*\*\*\*',
            r'START OF TH[EI]S? PROJECT GUTENBERG EBOOK',
        ]
        
        start_pos = -1
        for pattern in start_patterns:
            match = re.search(pattern, content, re.IGNORECASE | re.MULTILINE)
            if match:
                start_pos = match.end()
                print(f"    ✓ Found start marker at position {match.start()}")
                break
        
        # Find the end marker
        end_patterns = [
            r'\*\*\* END OF TH[EI]S? PROJECT GUTENBERG EBOOK[^*]*\*\*\*',
            r'\*\*\*END OF TH[EI]S? PROJECT GUTENBERG EBOOK[^*]*\*\*\*',
            r'END OF TH[EI]S? PROJECT GUTENBERG EBOOK',
        ]
        
        end_pos = len(content)
        for pattern in end_patterns:
            match = re.search(pattern, content, re.IGNORECASE | re.MULTILINE)
            if match:
                end_pos = match.start()
                print(f"    ✓ Found end marker at position {match.start()}")
                break
        
        if start_pos == -1:
            print("    ⚠️  No start marker found, keeping full content")
            return content
        
        extracted = content[start_pos:end_pos].strip()
        print(f"    ✓ Extracted main content ({len(extracted):,} characters)")
        return extracted
    import re

    def _remove_table_of_contents(self, content: str, force=False) -> str:
        """
        Remove table of contents sections, with confirmation.
        Stops TOC removal when a line repeats in the TOC itself (to preserve first real headers).
        """
        lines = content.splitlines()
        
        # Patterns that mark a TOC header
        toc_header = re.compile(r'^\s*(TABLE OF )?CONTENTS?\.?$|^INDEX\.?$', re.IGNORECASE)
        
        # Patterns that look like TOC entries
        toc_entry_patterns = [
            r'^\s*CHAPTER\s+[IVXLC]+\.*\s*$',  # CHAPTER I.
            r'^\s*CHAPTER\s+\d+\.*\s*$',       # CHAPTER 1.
            r'^\s*[A-Z][A-Z\s\-\’;:,]+$',      # ALL CAPS titles
            r'^\s*.+\s+\d+\s*$',               # Title followed by a page number
            r'^\s*[IVXLC]+\.?\s+[A-Z]',        # I. TITLE
            r'^\s*\d+\.?\s+[A-Z]',             # 1. TITLE
            r'^[\s─]+$',                        # divider lines
        ]
        toc_entry = re.compile("|".join(toc_entry_patterns))
        
        in_toc = False
        toc_removed = False
        skipped_lines = []
        cleaned_lines = []
        seen_in_toc = set()

        for line in lines:
            stripped = line.strip()
            
            # Start TOC when hitting a TOC header
            if not in_toc and toc_header.match(stripped):
                in_toc = True
                toc_removed = True
                skipped_lines.append(line)
                seen_in_toc.add(stripped)
                continue
            
            if in_toc:
                # Stop TOC if this line repeats in the TOC itself
                if stripped in seen_in_toc:
                    in_toc = False
                    cleaned_lines.append(line)
                    continue
                
                # Stop if line looks like prose (long or mixed case with punctuation)
                if len(stripped) > 80 or re.search(r'[a-z].*[a-z].*[.,;]', stripped):
                    in_toc = False
                    cleaned_lines.append(line)
                    continue
                
                # Skip lines that match TOC entry patterns or are empty
                if not stripped or toc_entry.match(stripped):
                    skipped_lines.append(line)
                    seen_in_toc.add(stripped)
                    continue
                
                # Unexpected format → stop TOC
                in_toc = False
                cleaned_lines.append(line)
                continue
            
            else:
                cleaned_lines.append(line)
        
        # Confirmation prompt
        if toc_removed and skipped_lines:
            preview = "\n".join(skipped_lines[:40])
            print("\n⚠️ Possible Table of Contents detected:\n")
            print(preview)
            if not force:
                choice = input("\nDelete this section? (y/n): ").strip().lower()
                if choice != "y":
                    print("ℹ️ Keeping TOC")
                    return content
            print("✓ TOC removed")
            return "\n".join(cleaned_lines)
        
        print("ℹ️ No TOC detected")
        return content
     
    def _fixing_font_styling(self, text: str) -> tuple[str, int]:
        """
        Remove all leading/trailing underscores from substrings that start and end with underscores.
        Returns (new_text, num_replacements).
        """
        print("  🔄 Fixing font styling...")
        new_text, num_replacements = re.subn(r'_+([^_]+?)_+', r'\1', text)
        print(f"    ✓ Trimmed underscores from {num_replacements} words")
        return new_text
    
    def _fix_line_wrapping(self, content: str) -> str:
        """Fix artificial line breaks while preserving paragraphs"""
        print("  🔄 Fixing line wrapping...")
        
        # Split into paragraphs (double line breaks or more)
        paragraphs = re.split(r'\n\s*\n', content)
        fixed_paragraphs = []
        
        for para in paragraphs:
            if not para.strip():
                continue
            
            # Within each paragraph, join lines that aren't meant to be separate
            lines = para.split('\n')
            fixed_lines = []
            
            i = 0
            while i < len(lines):
                current_line = lines[i].strip()
                
                # Skip empty lines
                if not current_line:
                    i += 1
                    continue
                
                # Check if this looks like a chapter heading or section break
                if (re.match(r'^CHAPTER [IVXLC\d]+\.?', current_line, re.IGNORECASE) or
                    re.match(r'^[IVXLC\d]+\.?\s*$', current_line) or
                    len(current_line) < 80 and current_line.isupper()):
                    fixed_lines.append(current_line)
                    i += 1
                    continue
                
                # For regular text, merge with next line if it looks like a continuation
                merged = current_line
                i += 1
                
                while i < len(lines):
                    next_line = lines[i].strip()
                    
                    # Stop if we hit an empty line or obvious break
                    if (not next_line or 
                        re.match(r'^CHAPTER [IVXLC\d]+\.?', next_line, re.IGNORECASE) or
                        len(next_line) < 80 and next_line.isupper()):
                        break

                    # Join hyphenated wrap: "travell-" + "ing" -> "travelling"
                    if merged.endswith('-') and next_line and next_line[0].islower():
                        merged = merged[:-1] + next_line
                        i += 1
                        continue
                    
                    # If current line doesn't end with sentence-ending punctuation,
                    # and next line doesn't start with capital letter or indent,
                    # probably a line wrap
                    # if (not merged_line.endswith(('.', '!', '?', ':', ';', '"', "'")) and
                    #     next_line and not next_line[0].isupper() and
                    #     not next_line.startswith('    ')):

                    # Default: join with space
                    merged += ' ' + next_line
                    i += 1

                    # else:
                    #     break
                
                fixed_lines.append(merged)
            
            if fixed_lines:
                fixed_paragraphs.append('\n'.join(fixed_lines))

        result = '\n\n'.join(fixed_paragraphs)
        print(f"    ✓ Line wrapping fixed")
        return result
    
    def _handle_special_characters(self, content: str) -> str:
        """Handle special characters that might not display in TextMeshPro"""
        print("  🔤 Checking for special characters...")

        acceptable_chars = {
            '“': '"',  # Left double quote
            '”': '"',  # Right double quote
            '’': "'",  # Left single quote
            '‘': "'",  # Right single quote
            '—': '--', # Em dash
            '–': '-',  # En dash
            '…': '...',# Ellipsis
            '®': '(R)',# Registered trademark
            '©': '(C)',# Copyright
            '™': '(TM)',# Trademark
            '½': '1/2',
            '¼': '1/4',
            '¾': '3/4',
            'É': 'E',
            'é': 'e',
            'è': 'e', 
            'ê': 'e',
            'ë': 'e',
            'à': 'a',
            'á': 'a',
            'â': 'a',
            'ä': 'a',
            'ù': 'u',
            'ú': 'u',
            'û': 'u',
            'ü': 'u',
            'ì': 'i',
            'í': 'i',
            'î': 'i',
            'ï': 'i',
            'ò': 'o',
            'ó': 'o',
            'ô': 'o',
            'ö': 'o',
            'ñ': 'n',
            'ç': 'c',
            '\n': '',
            '\r': '',
            '\t': '',
        }

        # Find non-ASCII characters
        special_chars = set()
        for char in content:
            if ord(char) > 127 and char not in acceptable_chars:
                special_chars.add(char)
        
        if not special_chars:
            print("    ✓ No special characters found")
            return content
        
        print(f"    ⚠️  Found {len(special_chars)} special characters:")
         
        # Automatic replacements
        replacements = {}
        auto_replaced = []
        for char in list(special_chars):
            if char in replacements:
                content = content.replace(char, replacements[char])
                auto_replaced.append(f"'{char}' → '{replacements[char]}'")
                special_chars.remove(char)
        
        if auto_replaced:
            print(f"    ✓ Auto-replaced: {', '.join(auto_replaced)}")
        
        # Interactive replacements for remaining characters
        if special_chars:
            print(f"    🤔 {len(special_chars)} characters need your input:")
            for char in sorted(special_chars):
                # Show context for the character
                examples = []
                lines = content.split('\n')
                for line in lines[:50]:  # Check first 50 lines
                    if char in line:
                        start = line.find(char)
                        context_start = max(0, start - 20)
                        context_end = min(len(line), start + 21)
                        context = line[context_start:context_end]
                        examples.append(f"...{context}...")
                        if len(examples) >= 2:  # Show up to 2 examples
                            break
                
                print(f"\n      Character: '{char}' (Unicode: {ord(char)})")
                if examples:
                    print(f"      Context: {examples[0]}")
                
                while True:
                    replacement = input(f"      Replace '{char}' with (or press Enter to keep): ").strip()
                    if replacement == char:
                        print("      (Replacement same as original, skipping)")
                        break
                    elif replacement == '':
                        print(f"      Keeping '{char}' unchanged")
                        break
                    else:
                        content = content.replace(char, replacement)
                        print(f"      ✓ Replaced '{char}' → '{replacement}'")
                        break
        
        print("    ✅ Special character processing complete")
        return content
    
    def _remove_bracketed_asides(self, text: str) -> str:
        """
        Remove any substrings enclosed in square brackets. If a bracketed aside
        appears as its own paragraph (with line breaks before and after), remove
        those surrounding breaks as well to avoid leaving extra blank space.
        """
        print("  ✂️  Removing bracketed asides...")

        original_length = len(text)

        # 1) Collapse blankline + [aside] + blankline → single blankline
        pattern_surrounded = r"(?m)\n\s*\n\s*\[[^\[\]\n]*\]\s*\n\s*\n"
        text, num_surrounded = re.subn(pattern_surrounded, "\n\n", text)

        # 2) Remove standalone bracketed lines (including their trailing newline)
        pattern_standalone = r"(?m)^\s*\[[^\[\]\n]*\]\s*\n?"
        text, num_standalone = re.subn(pattern_standalone, "", text)

        # 3) Remove inline bracketed asides within prose
        pattern_inline = r"\[[^\[\]\n]*\]"
        text, num_inline = re.subn(pattern_inline, "", text)

        removed_count = num_surrounded + num_standalone + num_inline
        print(f"    ✓ Removed {removed_count} bracketed occurrence(s)"
              f" (surrounded: {num_surrounded}, standalone: {num_standalone}, inline: {num_inline})")

        print(f"    ✓ Reduced size by {original_length - len(text):,} characters")
        return text

    def _final_cleanup(self, content: str) -> str:
        """Final cleanup and normalization"""
        print("  🧹 Final cleanup...")
        
        # Remove excessive whitespace
        content = re.sub(r'\n{3,}', '\n\n', content)  # Max 2 consecutive newlines
        content = re.sub(r'[ \t]+', ' ', content)     # Multiple spaces to single space
        content = re.sub(r'[ \t]*\n', '\n', content)  # Remove trailing whitespace
        
        # Ensure consistent paragraph spacing
        content = content.strip()
        
        print("    ✓ Cleanup complete")
        return content
    
    def run(self):
        """Main interactive loop"""
        print("=== Project Gutenberg Downloader for Unity ===")
        print(f"Working directory: {self.base_dir.absolute()}")
        print(f"Raw files: {self.raw_dir}")
        print(f"Formatted files: {self.formatted_dir}")
        print()
        
        while True:
            print("\nOptions:")
            print("  [D] Download a book")
            print("  [F] Format an existing file for Unity")
            print("  [Q] Quit")
            
            choice = input("Choose an option: ").strip().upper()
            
            if choice == 'Q':
                print("Goodbye!")
                break
            elif choice == 'D':
                self.download_workflow()
            elif choice == 'F':
                self.format_workflow()
            else:
                print("Invalid option. Please choose D, F, or Q.")
    
    def download_workflow(self):
        """Handle the book download workflow"""
        while True:
            title = input("\nEnter book title to search for (or 'back' to return): ").strip()
            
            if title.lower() == 'back':
                break
            
            if not title:
                print("Please enter a title.")
                continue
            
            # Search for books
            books = self.search_books(title)
            
            # Let user select a book
            selected_book = self.display_search_results(books)
            
            if selected_book is None:
                continue  # User chose to search again
            
            # Download the selected book
            downloaded_file = self.download_book(selected_book)
            
            if downloaded_file:
                # Ask if user wants to format it now
                format_choice = input("\nWould you like to format this book for Unity now? (y/n): ").strip().lower()
                if format_choice in ['y', 'yes']:
                    self.format_for_unity(downloaded_file)
                
                # Ask if user wants to download another
                continue_choice = input("\nDownload another book? (y/n): ").strip().lower()
                if continue_choice not in ['y', 'yes']:
                    break
            else:
                print("Download failed. Please try again.")
    
    def format_workflow(self):
        """Handle formatting of existing files"""
        # List available raw files
        raw_files = list(self.raw_dir.glob("*.txt"))
        
        if not raw_files:
            print(f"\nNo text files found in {self.raw_dir}")
            print("Download some books first!")
            return
        
        print(f"\nAvailable files in {self.raw_dir}:")
        for i, file in enumerate(raw_files, 1):
            print(f"  {i:2d}. {file.name}")
        
        while True:
            try:
                choice = input(f"\nSelect a file to format (1-{len(raw_files)}) or 0 to go back: ").strip()
                
                if choice == '0':
                    break
                
                choice_num = int(choice)
                if 1 <= choice_num <= len(raw_files):
                    selected_file = raw_files[choice_num - 1]
                    self.format_for_unity(selected_file)
                    break
                else:
                    print("Invalid selection. Please try again.")
                    
            except ValueError:
                print("Please enter a number.")
            except KeyboardInterrupt:
                print("\nOperation cancelled.")
                break


def main():
    # Allow user to specify working directory
    if len(sys.argv) > 1:
        work_dir = sys.argv[1]
        if not os.path.exists(work_dir):
            print(f"Directory '{work_dir}' does not exist.")
            sys.exit(1)
    else:
        work_dir = "."
    
    try:
        downloader = GutenbergDownloader(work_dir)
        downloader.run()
    except KeyboardInterrupt:
        print("\n\nGoodbye!")
    except Exception as e:
        print(f"An error occurred: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
