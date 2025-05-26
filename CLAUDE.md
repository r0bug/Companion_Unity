# Important Repository Structure Information

## ⚠️ CRITICAL WARNING ⚠️
**NEVER DELETE FILES IN THESE REPOSITORIES WITHOUT EXPLICIT USER PERMISSION**
**NEVER RUN `git clean`, `rm -rf`, or any destructive commands without confirmation**
**ALWAYS VERIFY the current directory before any Git operations**

## Project Organization
The `/home/robug/projects` directory contains multiple INDEPENDENT Git repositories. Each subdirectory is its own separate project with its own Git repository:

- `/home/robug/projects/AlvinScan` - AlvinScan inventory management (git@github.com:r0bug/AlvinScan.git) - Python
- `/home/robug/projects/Ebaytools` - eBay tools project (git@github.com:r0bug/Ebaytools.git) - Python
- `/home/robug/projects/EbaytoolsCompanion` - eBay tools companion app (git@github.com:r0bug/EbaytoolsCompanion.git) - Kotlin/Android
- `/home/robug/projects/YFEvents` - YF Events project (git@github.com:r0bug/YFEvents.git) - PHP
- `/home/robug/projects/companion_unity` - Unity version of eBay tools companion app - C#/Unity

**Note:** `temp_companion` is a duplicate of EbaytoolsCompanion and should be ignored/removed

## Critical Rules
1. **NEVER** initialize Git in parent directories (especially not in `/home/robug` or `/home/robug/projects`)
2. **ALWAYS** verify you're in the correct project directory before any Git operations
3. **EACH PROJECT** has its own separate Git repository - they are NOT related
4. **CHECK** the current directory with `pwd` before running Git commands
5. **VERIFY** remote URLs match the project name to avoid cross-contamination

## Common Issues to Avoid
- Do NOT run `git init` in `/home/robug` or `/home/robug/projects`
- Do NOT mix files between projects
- Do NOT push one project's code to another project's repository
- Always use `cd /home/robug/projects/[PROJECT_NAME]` before Git operations
- NEVER delete files without explicit permission - use `git status` to check before any changes
- NEVER run `git clean -fd` or similar destructive commands
- If files appear deleted, use `git checkout -- .` to restore them from the repository

## Repository Sync Verification Commands
To verify all repositories are in sync with GitHub, run these commands:
```bash
cd /home/robug/projects/AlvinScan && git fetch && git status
cd /home/robug/projects/Ebaytools && git fetch && git status
cd /home/robug/projects/EbaytoolsCompanion && git fetch && git status
cd /home/robug/projects/YFEvents && git fetch && git status
cd /home/robug/projects/companion_unity && git fetch && git status
```

## Git Configuration
- Author: John Storlie <john@robug.com>
- Each project should maintain its own `.git` directory
- Use project-specific Git configuration when needed

## SSH/Authentication Note
SSH keys may need to be configured for GitHub access. If SSH fails, the repositories can also be accessed via HTTPS with appropriate credentials.

### SSH Key Setup
1. SSH keys should be stored in `~/.ssh/` directory
2. GitHub's host keys are stored in `~/.ssh/known_hosts`
3. If SSH authentication fails, you may need to:
   - Add the SSH public key to GitHub account
   - Or use HTTPS with a personal access token
   - Or check if there's an existing SSH key deployment key


## GitHub Safety Rules (From Claude's Instructions)

### Key Guidelines to Prevent GitHub Issues:

1. **Never update git config** - Do not modify git configuration settings

2. **Only commit when explicitly asked** - Never commit changes unless specifically requested to prevent unwanted commits

3. **Commit message format** - When creating commits:
   - Include a meaningful message explaining the purpose of changes
   - End with: "🤖 Generated with [Claude Code](https://claude.ai/code)"
   - Add Co-Authored-By: Claude <noreply@anthropic.com>

4. **Don't push to remote** - Never push changes to remote repositories

5. **No interactive git commands** - Avoid commands requiring user input (like `git rebase -i`)

6. **Check before committing** - Run `git status` and `git diff` to review changes before committing

7. **Never use destructive commands** without explicit permission:
   - No `git clean -fd`
   - No `rm -rf`
   - No file deletions without confirmation

8. **Recovery commands** - If files appear deleted, use `git checkout -- .` to restore from repository

These safeguards ensure no accidental modification of git history or pushing unwanted changes.

## Project-Specific Information

This is the **companion_unity** project - a Unity-based port of the EbaytoolsCompanion Android app.