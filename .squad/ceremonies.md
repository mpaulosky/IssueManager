# Squad Ceremonies

## Defined Ceremonies

### Pre-Sprint Planning
- **Trigger:** manual ("run sprint planning", "plan the sprint")
- **When:** before
- **Facilitator:** Aragorn
- **Participants:** Aragorn, Sam, Legolas, Gimli, Boromir
- **Purpose:** Review open issues, prioritize, assign squad labels

### Build Repair Check
- **Trigger:** automatic, when: "before push" or "before PR"
- **When:** before
- **Facilitator:** Aragorn
- **Participants:** Aragorn (runs build-repair prompt)
- **Purpose:** Ensure zero errors, zero warnings, all tests pass before pushing

### Retro
- **Trigger:** manual ("run retro", "retrospective")
- **When:** after
- **Facilitator:** Aragorn
- **Participants:** all
- **Purpose:** What went well, what didn't, action items

### Code Review
- **Trigger:** automatic, when PR is opened
- **When:** after
- **Facilitator:** Aragorn
- **Participants:** Aragorn (reviewer), original author (locked out of their own revision if rejected)
- **Purpose:** Quality gate before merge
